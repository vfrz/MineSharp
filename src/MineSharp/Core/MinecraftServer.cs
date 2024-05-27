using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MineSharp.Commands;
using MineSharp.Configuration;
using MineSharp.Content;
using MineSharp.Entities;
using MineSharp.Entities.Mobs;
using MineSharp.Extensions;
using MineSharp.Network;
using MineSharp.Network.Packets;
using MineSharp.Saves;
using MineSharp.World;

namespace MineSharp.Core;

public class MinecraftServer : IDisposable
{
    private const int ClientTimeoutInSeconds = 10;

    private readonly SemaphoreSlim _addClientSemaphore = new(1, 1);

    public IEnumerable<RemoteClient> RemoteClients => _remoteClients.Values;
    private readonly ConcurrentDictionary<string, RemoteClient> _remoteClients;

    public int PlayerCount => _remoteClients.Count;

    private readonly Socket _socket;
    private readonly PacketDispatcher _packetDispatcher;
    public ServerConfiguration Configuration { get; }
    private readonly ILogger<MinecraftServer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly CommandHandler _commandHandler;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    
    public MinecraftWorld World { get; private set; } = null!;

    public EntityManager EntityManager { get; }
    public Looper Looper { get; }

    private bool _saveOnNextLoop;

    public MinecraftServer(PacketDispatcher packetDispatcher,
        IOptions<ServerConfiguration> configuration,
        ILogger<MinecraftServer> logger,
        IServiceProvider serviceProvider,
        CommandHandler commandHandler,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _packetDispatcher = packetDispatcher;
        Configuration = configuration.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _commandHandler = commandHandler;
        _hostApplicationLifetime = hostApplicationLifetime;
        _remoteClients = new ConcurrentDictionary<string, RemoteClient>();

        EntityManager = new EntityManager(this);

        Looper = new Looper(TimeSpan.FromMilliseconds(50), ProcessAsync);

        var ip = new IPEndPoint(IPAddress.Any, configuration.Value.Port);
        _socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _socket.Bind(ip);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Server initializing...");

        SaveManager.Initialize();

        if (SaveManager.IsWorldSaved())
        {
            var worldSaveData = SaveManager.LoadWorld();
            World = MinecraftWorld.FromSaveData(this, worldSaveData);
        }
        else
        {
            World = MinecraftWorld.New(this, Configuration.WorldSeed.GetValueOrDefault(new Random().Next()));
            SaveManager.SaveWorld(World.GetSaveData());
        }

        await World.LoadInitialChunksAsync();

        Looper.RegisterLoop(TimeSpan.FromSeconds(1), EntityManager.ProcessPickupItemsAsync);
        Looper.RegisterLoop(TimeSpan.FromSeconds(5), SendKeepAlivePacketsAsync);
        Looper.RegisterLoop(TimeSpan.FromSeconds(1), World.SendTimeUpdateAsync);
        Looper.RegisterLoop(TimeSpan.FromSeconds(1), async token =>
        {
            await Parallel.ForEachAsync(RemoteClients, token, async (client, _) =>
            {
                if (client.Player is not null && !client.Player.Respawning)
                    await client.UpdateLoadedChunksAsync();
            });
        });
        Looper.RegisterLoop(TimeSpan.FromMinutes(Configuration.AutomaticSaveIntervalInMinutes),
            _ => TriggerSave(), executeOnStart: false);

        RegisterDefaultCommands();

        _logger.LogInformation("Server initialized");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Server starting...");

        World.Start();

        Looper.Start();

        _socket.Listen();
        _ = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var socket = await _socket.AcceptAsync(cancellationToken);
                _ = HandleClientAsync(socket, cancellationToken);
            }
        }, cancellationToken);

        _logger.LogInformation("Server started on port: {port}", Configuration.Port);

        return Task.CompletedTask;
    }

    private async Task SendKeepAlivePacketsAsync(CancellationToken cancellationToken)
    {
        await BroadcastPacketAsync(new KeepAlivePacket(), readyOnly: true);
    }

    public async Task ShutdownAsync(CancellationToken cancellationToken)
    {
        await DisconnectAllPlayersAsync("Server stopped.");

        _logger.LogInformation("Server stoping...");
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
        _socket.Dispose();
        _logger.LogInformation("Server stopped");

        await Looper.StopAsync();
        World.Stop();

        await SaveAsync(cancellationToken);
    }

    public void Stop()
    {
        _hostApplicationLifetime.StopApplication();
    }

    private void RegisterDefaultCommands()
    {
        _commandHandler.TryRegisterCommand("stop", async (server, _, _) =>
        {
            var delay = TimeSpan.FromSeconds(5);
            server.Looper.Schedule(delay, _ => server.Stop());
            await server.BroadcastChatAsync($"{ChatColors.Yellow}Server will stop in {(int) delay.TotalSeconds} seconds!");
            return true;
        });
        
        _commandHandler.TryRegisterCommand("id", async (_, client, _) =>
        {
            if (client is null)
                return true;
            await client.SendPacketAsync(new ChatMessagePacket
            {
                Message = $"Your entity id: {client.Player!.EntityId}"
            });
            return true;
        });

        _commandHandler.TryRegisterCommand("rain", async (server, _, _) =>
        {
            if (server.World.Raining)
                await server.World.StopRainAsync();
            else
                await server.World.StartRainAsync();
            return true;
        });

        _commandHandler.TryRegisterCommand("time", async (server, _, args) =>
        {
            var time = long.Parse(args[0]);
            await server.World.SetTimeAsync(time);
            return true;
        });

        _commandHandler.TryRegisterCommand("pos", async (_, client, _) =>
        {
            await client!.SendChatAsync(client.Player!.Position.ToString());
            return true;
        });

        _commandHandler.TryRegisterCommand("chunk", async (_, client, _) =>
        {
            await client!.SendChatAsync(client.GetCurrentChunk().ToString());
            return true;
        });

        _commandHandler.TryRegisterCommand("heal", async (_, client, _) =>
        {
            await client!.Player!.SetHealthAsync(20);
            return true;
        });

        _commandHandler.TryRegisterCommand("dmg", async (_, client, args) =>
        {
            var value = args[0].ParseInt();
            await client!.Player!.SetHealthAsync((short) (client.Player.Health - value));
            return true;
        });

        _commandHandler.TryRegisterCommand("kill", async (_, client, _) =>
        {
            await client!.Player!.SetHealthAsync(0);
            return true;
        });

        _commandHandler.TryRegisterCommand("yaw", async (server, client, _) =>
        {
            await server.BroadcastChatAsync($"Yaw: {client!.Player!.Yaw}");
            return true;
        });

        _commandHandler.TryRegisterCommand("mob", async (server, client, args) =>
        {
            await server.SpawnMobAsync((MobType) byte.Parse(args[0]), client!.Player!.Position.ToVector3i());
            return true;
        });

        _commandHandler.TryRegisterCommand("chest", async (_, client, _) =>
        {
            await client!.SendPacketAsync(new OpenWindowPacket
            {
                WindowId = 42,
                InventoryType = 0,
                Slots = 27 * 2,
                WindowTitle = "Ahahah"
            });
            return true;
        });

        _commandHandler.TryRegisterCommand("give", async (_, client, args) =>
        {
            var itemId = (ItemId) short.Parse(args[0]);
            var count = args.Length > 1 ? byte.Parse(args[1]) : ItemInfoProvider.Get(itemId).StackMax;
            var metadata = args.Length > 2 ? short.Parse(args[2]) : (byte) 0;

            return await client!.Player!.TryGiveItemAsync(new ItemStack(itemId, count, metadata));
        });

        _commandHandler.TryRegisterCommand("clear", async (_, client, _) =>
        {
            await client!.Player!.ClearInventoryAsync();
            return true;
        });

        _commandHandler.TryRegisterCommand("save", async (server, client, _) =>
        {
            await server.SaveAsync(CancellationToken.None);
            await client!.SendChatAsync($"{ChatColors.Green}Saved successfully");
            return true;
        });

        _commandHandler.TryRegisterCommand("gc", (_, _, _) =>
        {
            GC.Collect();
            return Task.FromResult(true);
        });

        _commandHandler.TryRegisterCommand("tp", async (server, client, args) =>
        {
            Vector3d target;

            if (args.Length == 1)
            {
                var otherPlayer = server.GetRemoteClientByUsername(args[0]);
                if (otherPlayer?.Player is null)
                {
                    await client!.SendChatAsync($"Can't find player named {args[0]}");
                    return false;
                }

                target = otherPlayer.Player.Position;
            }
            else if (args.Length == 2)
            {
                var targetX = int.Parse(args[0]);
                var targetZ = int.Parse(args[1]);
                var height = await server.World.GetHighestBlockHeightAsync(new Vector2i(targetX, targetZ));
                target = new Vector3d(targetX + 0.5, height + 1.1, targetZ + 0.5);
            }
            else
            {
                return false;
            }

            var player = client!.Player!;
            player.Position = target;
            await client.LoadChunkAsync(Chunk.GetChunkPositionForWorldPosition(player.Position));
            await client.SendPacketAsync(new PlayerPositionAndLookServerPacket
            {
                X = player.Position.X,
                Y = player.Position.Y,
                Z = player.Position.Z,
                Pitch = player.Pitch,
                Yaw = player.Yaw,
                OnGround = player.OnGround,
                Stance = player.Position.Y + Player.Height
            });
            return true;
        });
    }

    public RemoteClient? GetRemoteClientByUsername(string username)
    {
        return _remoteClients.Values.FirstOrDefault(remoteClient => remoteClient.Player?.Username == username);
    }

    public ILogger<T> GetLogger<T>()
    {
        return _serviceProvider.GetRequiredService<ILogger<T>>();
    }

    private async Task ProcessAsync(TimeSpan elapsed, CancellationToken cancellationToken)
    {
        // Pass cancellation token
        await _packetDispatcher.HandlePacketsAsync();

        foreach (var remoteClient in RemoteClients)
        {
            var player = remoteClient.Player;
            if (player is null || player.PositionDirty is false || player.Health == 0)
                continue;

            // Kill player if off map
            if (player.Position.Y < -50)
            {
                await player.SetHealthAsync(0);
            }

            await BroadcastPacketAsync(new EntityTeleportPacket
            {
                EntityId = player.EntityId,
                X = player.Position.X.ToAbsolutePosition(),
                Y = player.Position.Y.ToAbsolutePosition(),
                Z = player.Position.Z.ToAbsolutePosition(),
                Yaw = MinecraftMath.RotationFloatToSByte(player.Yaw),
                Pitch = MinecraftMath.RotationFloatToSByte(player.Pitch)
            }, remoteClient, readyOnly: true);
            player.PositionDirty = false;
        }

        await World.ProcessAsync(elapsed);
        await EntityManager.ProcessAsync(elapsed);

        if (_saveOnNextLoop)
        {
            await SaveAsync(cancellationToken);
            _saveOnNextLoop = false;
        }
    }

    public async Task DisconnectAllPlayersAsync(string message)
    {
        foreach (var remoteClient in RemoteClients)
        {
            await SavePlayerAsync(remoteClient);
        }

        await BroadcastPacketAsync(new PlayerDisconnectPacket
        {
            Reason = message
        });
    }

    private Task SavePlayerAsync(RemoteClient client)
    {
        client.Player?.Save();
        return Task.CompletedTask;
    }

    public async Task BroadcastPacketAsync(IServerPacket packet, RemoteClient? except = null, bool readyOnly = false)
    {
        await using var writer = new PacketWriter(packet.PacketId);
        packet.Write(writer);
        var data = writer.ToByteArray();
        await Parallel.ForEachAsync(RemoteClients, async (client, _) =>
        {
            if (client == except || (readyOnly && client.State != RemoteClient.ClientState.Ready))
                return;
            await client.TrySendAsync(data);
        });
    }

    public async Task BroadcastChatAsync(string message, RemoteClient? except = null)
    {
        await BroadcastPacketAsync(new ChatMessagePacket
        {
            Message = message
        }, except);
    }

    public async Task<IMobEntity> SpawnMobAsync(MobType type, Vector3i position)
    {
        IMobEntity mob = type switch
        {
            MobType.Creeper => new Creeper(this),
            MobType.Skeleton => new Skeleton(this),
            MobType.Spider => new Spider(this),
            MobType.GiantZombie => new GiantZombie(this),
            MobType.Zombie => new Zombie(this),
            MobType.Slime => new Slime(this),
            MobType.Ghast => new Ghast(this),
            MobType.ZombiePigman => new ZombiePigman(this),
            MobType.Pig => new Pig(this),
            MobType.Sheep => new Sheep(this),
            MobType.Cow => new Cow(this),
            MobType.Chicken => new Chicken(this),
            MobType.Squid => new Squid(this),
            MobType.Wolf => new Wolf(this),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        return await SpawnMobAsync(mob, position);
    }

    public async Task<IMobEntity> SpawnMobAsync(IMobEntity mob, Vector3i position)
    {
        mob.Position = position.ToVector3();
        mob.Pitch = 0;
        mob.Yaw = 0;
        mob.OnGround = true;
        await mob.SetHealthAsync(mob.MaxHealth);
        EntityManager.RegisterEntity(mob);
        await BroadcastPacketAsync(new MobSpawnPacket
        {
            EntityId = mob.EntityId,
            X = mob.Position.X.ToAbsolutePosition(),
            Y = mob.Position.Y.ToAbsolutePosition(),
            Z = mob.Position.Z.ToAbsolutePosition(),
            Pitch = MinecraftMath.RotationFloatToSByte(mob.Pitch),
            Yaw = MinecraftMath.RotationFloatToSByte(mob.Yaw),
            Type = mob.Type,
            MetadataContainer = mob.MetadataContainer
        });
        return mob;
    }

    private async Task HandleClientAsync(Socket socket, CancellationToken cancellationToken)
    {
        var remoteClient = new RemoteClient(socket, this);

        await _addClientSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (PlayerCount >= Configuration.MaxPlayers)
            {
                await remoteClient.SendPacketAsync(new PlayerDisconnectPacket
                {
                    Reason = $"Server is full: {_remoteClients.Count}/{Configuration.MaxPlayers}"
                });
                _logger.LogInformation("Client {networkId} tried to connect but the server is full",
                    remoteClient.NetworkId);
                await remoteClient.DisconnectSocketAsync();
                return;
            }

            AddRemoteClient(remoteClient);
        }
        finally
        {
            _addClientSemaphore.Release();
        }

        _logger.LogInformation("Client {networkId} connected", remoteClient.NetworkId);

        var context = new ClientPacketHandlerContext(this, remoteClient);

        var pipe = new Pipe();

        try
        {
            var writing = FillPipeAsync(socket, pipe, cancellationToken);
            var reading = ReadPipeAsync(context, pipe, cancellationToken);

            await Task.WhenAll(reading, writing);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Client with network id: {0} has timed out", remoteClient.NetworkId);
            try
            {
                await remoteClient.SendPacketAsync(new PlayerDisconnectPacket
                {
                    Reason = "Disconnected because you was afk"
                });
            }
            catch
            {
                // ignored
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception({exceptionType}): {exceptionMessage}", ex.GetType().ToString(), ex.Message);
            if (!socket.Connected)
                throw;
        }
        finally
        {
            if (remoteClient.Player is not null)
            {
                _logger.LogInformation("Player {username} ({networkId}) has left the server", remoteClient.Player.Username, remoteClient.NetworkId);
                remoteClient.Player.Save();
            }

            _logger.LogInformation("Client {networkId} disconnected", remoteClient.NetworkId);
            await RemoveRemoteClientAsync(remoteClient);
        }
    }

    public void TriggerSave()
    {
        _saveOnNextLoop = true;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Saving everything...");

        await World.SaveAsync();

        foreach (var remoteClient in RemoteClients)
        {
            await SavePlayerAsync(remoteClient);
        }

        _logger.LogInformation("Saved successfully");
    }

    private void AddRemoteClient(RemoteClient remoteClient)
    {
        if (!_remoteClients.TryAdd(remoteClient.NetworkId, remoteClient))
            throw new Exception("Failed to add client to list");
    }

    private async Task RemoveRemoteClientAsync(RemoteClient remoteClient)
    {
        if (!_remoteClients.Remove(remoteClient.NetworkId, out _))
            throw new Exception("Failed to remove client");
        remoteClient.Dispose();

        await PlayerDisconnectedAsync(remoteClient);
    }

    private async Task PlayerDisconnectedAsync(RemoteClient remoteClient)
    {
        if (remoteClient.Player is not null)
        {
            var leftMessage = $"{ChatColors.Blue}{remoteClient.Player.Username} {ChatColors.White}has left the server!";
            await BroadcastChatAsync(leftMessage, remoteClient);

            await BroadcastPacketAsync(new DestroyEntityPacket
            {
                EntityId = remoteClient.Player.EntityId
            }, remoteClient);

            EntityManager.FreeEntity(remoteClient.Player);
        }
    }

    private async Task FillPipeAsync(Socket socket, Pipe pipe, CancellationToken cancellationToken)
    {
        while (true)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(ClientTimeoutInSeconds));

            // Allocate at least 65535 bytes from the PipeWriter. (Maximum tcp packet size)
            var memory = pipe.Writer.GetMemory(65535);

            var bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, cts.Token);
            if (bytesRead == 0)
                break;

            // Tell the PipeWriter how much was read from the Socket.
            pipe.Writer.Advance(bytesRead);

            // Make the data available to the PipeReader.
            var result = await pipe.Writer.FlushAsync(cts.Token);

            if (result.IsCompleted)
                break;
        }

        // By completing PipeWriter, tell the PipeReader that there's no more data coming.
        await pipe.Writer.CompleteAsync();
    }

    private async Task ReadPipeAsync(ClientPacketHandlerContext context, Pipe pipe, CancellationToken cancellationToken)
    {
        while (true)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(ClientTimeoutInSeconds));

            var result = await pipe.Reader.ReadAsync(cts.Token);
            var buffer = result.Buffer;

            if (buffer.Length > 0)
            {
                var position = _packetDispatcher.DispatchPacket(buffer, context);
                pipe.Reader.AdvanceTo(position);
            }

            // Stop reading if there's no more data coming.
            if (result.IsCompleted)
                break;
        }

        // Mark the PipeReader as complete.
        await pipe.Reader.CompleteAsync();
    }

    public void Dispose()
    {
        World.Dispose();
        _addClientSemaphore.Dispose();
    }
}
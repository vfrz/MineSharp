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
using MineSharp.Numerics;
using MineSharp.Plugins;
using MineSharp.Saves;
using MineSharp.World;

namespace MineSharp.Core;

public class MinecraftServer : IServer, IDisposable
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

    ICommands IServer.Commands => CommandManager;
    public CommandManager CommandManager { get; }

    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    IWorld IServer.World => World;
    public MinecraftWorld World { get; private set; } = null!;

    public EntityManager EntityManager { get; }

    ILooper IServer.Looper => Looper;
    public Looper Looper { get; }

    public PluginManager PluginManager { get; }

    private bool _saveOnNextLoop;

    public MinecraftServer(PacketDispatcher packetDispatcher,
        IOptions<ServerConfiguration> configuration,
        ILogger<MinecraftServer> logger,
        IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _packetDispatcher = packetDispatcher;
        Configuration = configuration.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
        CommandManager = new CommandManager();
        _hostApplicationLifetime = hostApplicationLifetime;
        _remoteClients = new ConcurrentDictionary<string, RemoteClient>();

        EntityManager = new EntityManager(this);

        Looper = new Looper(TimeSpan.FromMilliseconds(50), ProcessAsync);

        PluginManager = new PluginManager(this);

        var ip = new IPEndPoint(IPAddress.Any, configuration.Value.Port);
        _socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _socket.Bind(ip);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Server initializing...");

        await PluginManager.ReloadPluginsAsync();

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
        Looper.RegisterLoop(TimeSpan.FromSeconds(1), World.BroadcastTimeUpdateAsync);
        Looper.RegisterLoop(TimeSpan.FromSeconds(1), async token =>
        {
            await Parallel.ForEachAsync(RemoteClients, token, async (client, _) =>
            {
                if (client.Player is not null && !client.Player.Respawning)
                    await client.Player.UpdateLoadedChunksAsync();
            });
        });
        Looper.RegisterLoop(TimeSpan.FromMinutes(Configuration.AutomaticSaveIntervalInMinutes),
            _ => TriggerSave(), executeOnStart: false);

        RegisterDefaultCommands();

        _logger.LogInformation("Server initialized");

        await PluginManager.CallAsync(plugin => plugin.OnServerInitializedAsync());
    }

    public async Task StartAsync(CancellationToken cancellationToken)
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

        await PluginManager.CallAsync(plugin => plugin.OnServerStartedAsync());
    }

    private async Task SendKeepAlivePacketsAsync(CancellationToken cancellationToken)
    {
        await BroadcastPacketAsync(new KeepAlivePacket(), readyClientsOnly: true);
    }

    public async Task ShutdownAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Server stopping...");

        await PluginManager.CallAsync(plugin => plugin.OnServerStoppingAsync());

        await DisconnectAllPlayersAsync("Server stopped.");

        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
        _socket.Dispose();

        await WaitForAllPlayersToBeDisconnectedAsync(TimeSpan.FromSeconds(5));

        await Looper.StopAsync();
        World.Stop();

        await SaveAsync(cancellationToken);

        _logger.LogInformation("Server stopped");

        await PluginManager.CallAsync(plugin => plugin.OnServerStoppedAsync());
    }

    public void Stop()
    {
        _hostApplicationLifetime.StopApplication();
    }

    private void RegisterDefaultCommands()
    {
        CommandManager.TryRegisterCommand("stop", async (server, _, args) =>
        {
            if (args.Length > 0)
            {
                var delay = TimeSpan.FromSeconds(int.Parse(args[0]));
                server.Looper.Schedule(delay, _ => server.Stop());
                await server.BroadcastChatAsync($"{ChatColors.Yellow}Server will stop in {(int) delay.TotalSeconds} seconds!");
            }
            else
            {
                server.Stop();
            }

            return true;
        });

        CommandManager.TryRegisterCommand("id", async (_, client, _) =>
        {
            if (client is null)
                return true;
            await client.SendChatAsync($"Your entity id: {client.Player!.EntityId}");
            return true;
        });

        CommandManager.TryRegisterCommand("rain", async (server, _, _) =>
        {
            if (server.World.Raining)
                await server.World.StopRainAsync();
            else
                await server.World.StartRainAsync();
            return true;
        });

        CommandManager.TryRegisterCommand("time", async (server, _, args) =>
        {
            var time = long.Parse(args[0]);
            await server.World.SetTimeAsync(time);
            return true;
        });

        CommandManager.TryRegisterCommand("pos", async (_, client, _) =>
        {
            await client!.SendChatAsync(client.Player!.Position.ToString());
            return true;
        });

        CommandManager.TryRegisterCommand("chunk", async (_, client, _) =>
        {
            if (client?.Player is not null)
                await client.SendChatAsync(client.Player.GetCurrentChunk().ToString());
            return true;
        });

        CommandManager.TryRegisterCommand("heal", async (_, client, _) =>
        {
            await client!.Player!.SetHealthAsync(client.Player.MaxHealth);
            return true;
        });

        CommandManager.TryRegisterCommand("dmg", async (_, client, args) =>
        {
            var value = args[0].ParseInt();
            await client!.Player!.SetHealthAsync((short) (client.Player.Health - value));
            return true;
        });

        CommandManager.TryRegisterCommand("kick", async (server, _, args) =>
        {
            var target = server.GetRemoteClientByUsername(args[0]);
            if (target?.Ready is true)
            {
                await target.KickAsync("You have been kicked.");
                return true;
            }

            return false;
        });

        CommandManager.TryRegisterCommand("kill", async (server, client, args) =>
        {
            if (args.Length > 0)
            {
                var target = server.GetRemoteClientByUsername(args[0]);
                if (target?.Ready is true)
                {
                    await target.Player!.SetHealthAsync(0);
                    return true;
                }

                return false;
            }

            await client!.Player!.SetHealthAsync(0);
            return true;
        });

        CommandManager.TryRegisterCommand("yaw", async (server, client, _) =>
        {
            await server.BroadcastChatAsync($"Yaw: {client!.Player!.Yaw}");
            return true;
        });

        CommandManager.TryRegisterCommand("give", async (server, _, args) =>
        {
            var username = args[0];
            var itemId = (ItemId) short.Parse(args[1]);
            var count = args.Length > 2 ? byte.Parse(args[2]) : ItemInfoProvider.Get(itemId).StackMax;
            var metadata = args.Length > 3 ? short.Parse(args[3]) : (byte) 0;

            var receiverRemoteClient = server.GetRemoteClientByUsername(username);
            if (receiverRemoteClient?.Ready is true)
                return await receiverRemoteClient.Player!.TryGiveItemAsync(new ItemStack(itemId, count, metadata));
            return false;
        });

        CommandManager.TryRegisterCommand("clear", async (_, client, _) =>
        {
            await client!.Player!.ClearInventoryAsync();
            return true;
        });

        CommandManager.TryRegisterCommand("save", async (server, client, _) =>
        {
            await server.SaveAsync(CancellationToken.None);
            await client!.SendChatAsync($"{ChatColors.Green}Saved successfully");
            return true;
        });

        CommandManager.TryRegisterCommand("tp", async (server, client, args) =>
        {
            Vector3<double> target;

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
                var height = await server.World.GetHighestBlockHeightAsync(new Vector2<int>(targetX, targetZ)) + 1;
                target = new Vector3<double>(targetX + 0.5, height, targetZ + 0.5);
            }
            else
            {
                return false;
            }

            var player = client!.Player!;
            await player.TeleportAsync(target);
            return true;
        });
    }


    IRemoteClient? IServer.GetRemoteClientByUsername(string username) => GetRemoteClientByUsername(username);

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
            if (player is null || player.PositionDirty is false || player.IsDead)
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
            }, remoteClient, readyClientsOnly: true);
            player.PositionDirty = false;
        }

        await World.ProcessAsync(elapsed);
        await EntityManager.ProcessAsync(elapsed);

        if (_saveOnNextLoop)
        {
            await SaveAsync(cancellationToken);
            _saveOnNextLoop = false;
        }

        await PluginManager.CallAsync(plugin => plugin.OnTickAsync(elapsed));
    }

    public async Task DisconnectAllPlayersAsync(string message)
    {
        await Parallel.ForEachAsync(RemoteClients, async (client, _) =>
        {
            SavePlayer(client);
            await client.KickAsync(message);
        });
    }

    private async Task WaitForAllPlayersToBeDisconnectedAsync(TimeSpan timeout)
    {
        try
        {
            await Task.Run(async () =>
            {
                while (_remoteClients.Count > 0)
                {
                    await Task.Delay(20);
                }
            }).WaitAsync(timeout);
        }
        catch (TimeoutException)
        {
            _logger.LogWarning($"Failed to disconnect all players after {(int) timeout.TotalSeconds} second(s)");
        }
    }

    private void SavePlayer(RemoteClient client)
    {
        client.Player?.Save();
    }

    public async Task BroadcastPacketAsync(IServerPacket packet, IRemoteClient? except = null, bool readyClientsOnly = false)
    {
        await using var writer = new PacketWriter(packet.PacketId);
        packet.Write(writer);
        var data = writer.ToByteArray();
        await Parallel.ForEachAsync(RemoteClients, async (client, _) =>
        {
            if (client == except || (readyClientsOnly && client.Ready is false))
                return;
            await client.TrySendAsync(data);
        });
    }

    public async Task BroadcastPacketForChunkAsync(IServerPacket packet, Vector2<int> chunkPosition,
        RemoteClient? except = null, bool readyClientsOnly = false)
    {
        await using var writer = new PacketWriter(packet.PacketId);
        packet.Write(writer);
        var data = writer.ToByteArray();

        var players = GetPlayersInChunk(chunkPosition)
            .Where(player => player.RemoteClient != except)
            .Where(player => !readyClientsOnly || player.RemoteClient.Ready);

        await Parallel.ForEachAsync(players, async (player, _) => await player.RemoteClient.TrySendAsync(data));
    }

    private IEnumerable<Player> GetPlayersInChunk(Vector2<int> chunkPosition)
        => RemoteClients
            .Where(c => c.Player is not null && c.Player.LoadedChunks.Contains(chunkPosition))
            .Select(c => c.Player!);

    public async Task BroadcastChatAsync(string message, IRemoteClient? except = null)
    {
        await BroadcastPacketAsync(new ChatMessagePacket
        {
            Message = message
        }, except);
    }

    public async Task<MobEntity> SpawnMobAsync(MobType type, Vector3<int> position)
    {
        MobEntity mob = type switch
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

    public async Task<MobEntity> SpawnMobAsync(MobEntity mob, Vector3<int> position)
    {
        mob.Position = position.ToVector3<double>();
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

        using (await _addClientSemaphore.EnterLockAsync(cancellationToken))
        {
            if (PlayerCount >= Configuration.MaxPlayers)
            {
                await remoteClient.KickAsync($"Server is full: {_remoteClients.Count}/{Configuration.MaxPlayers}");
                _logger.LogInformation("Client {networkId} tried to connect but the server is full", remoteClient.NetworkId);
                await remoteClient.DisconnectSocketAsync();
                return;
            }

            AddRemoteClient(remoteClient);
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
                await remoteClient.KickAsync("Disconnected because you was afk");
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

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Saving everything...");

        await World.SaveAsync(cancellationToken);

        foreach (var remoteClient in RemoteClients)
        {
            SavePlayer(remoteClient);
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
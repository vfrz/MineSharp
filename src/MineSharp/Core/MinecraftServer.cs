using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MineSharp.Commands;
using MineSharp.Configuration;
using MineSharp.Core.Packets;
using MineSharp.Entities;
using MineSharp.Entities.Mobs;
using MineSharp.Network;
using MineSharp.Network.Packets;
using MineSharp.World;

namespace MineSharp.Core;

public class MinecraftServer : IDisposable
{
    private const int ClientTimeoutInSeconds = 10;

    private readonly SemaphoreSlim _addClientSemaphore = new(1, 1);

    public IEnumerable<RemoteClient> RemoteClients => _remoteClients.Values;
    private readonly ConcurrentDictionary<string, RemoteClient> _remoteClients;

    private readonly Socket _socket;
    private readonly PacketDispatcher _packetDispatcher;
    public ServerConfiguration Configuration { get; }
    private readonly ILogger<MinecraftServer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly CommandHandler _commandHandler;

    public MinecraftWorld World { get; }
    public EntityManager EntityManager { get; }
    public Looper Looper { get; }

    public MinecraftServer(PacketDispatcher packetDispatcher,
        IOptions<ServerConfiguration> configuration,
        ILogger<MinecraftServer> logger,
        IServiceProvider serviceProvider,
        CommandHandler commandHandler)
    {
        _packetDispatcher = packetDispatcher;
        Configuration = configuration.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _commandHandler = commandHandler;
        _remoteClients = new ConcurrentDictionary<string, RemoteClient>();

        World = new MinecraftWorld(this, new Random().Next());

        EntityManager = new EntityManager(this);

        Looper = new Looper(TimeSpan.FromMilliseconds(50), ProcessAsync);

        RegisterDefaultCommands();

        var ip = new IPEndPoint(IPAddress.Any, configuration.Value.Port);
        _socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _socket.Bind(ip);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Server starting...");

        await World.LoadInitialChunksAsync();
        World.Start();

        Looper.RegisterLoop(TimeSpan.FromSeconds(1), EntityManager.ProcessPickupItemsAsync);
        Looper.RegisterLoop(TimeSpan.FromSeconds(5), SendKeepAlivePacketsAsync);
        Looper.RegisterLoop(TimeSpan.FromSeconds(1), World.SendTimeUpdateAsync);
        Looper.RegisterLoop(TimeSpan.FromSeconds(1), async (token) =>
        {
            await Parallel.ForEachAsync(RemoteClients, token, async (client, _) =>
            {
                if (client.Player is not null && !client.Player.Respawning)
                    await client.UpdateLoadedChunksAsync();
            });
        });

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
    }

    private async Task SendKeepAlivePacketsAsync(CancellationToken cancellationToken)
    {
        await BroadcastPacketAsync(new KeepAlivePacket(), readyOnly: true);
    }

    public async Task StopAsync()
    {
        await DisconnectAllPlayersAsync("Server stopped.");

        await Looper.StopAsync();
        World.Stop();

        _logger.LogInformation("Server stoping...");
        //_socket.Shutdown(SocketShutdown.Both);
        //_socket.Close();
        _socket.Dispose();
        _logger.LogInformation("Server stopped");
    }

    private void RegisterDefaultCommands()
    {
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

        _commandHandler.TryRegisterCommand("tp", async (server, client, args) =>
        {
            var target = new Vector3d();

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
                Stance = player.Position.Y + PlayerEntity.Height
            });
            return true;
        });
    }

    public RemoteClient? GetRemoteClientByUsername(string username)
    {
        return _remoteClients.Values.FirstOrDefault(remoteClient => remoteClient.Username == username);
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
                X = player.Position.X.ToAbsoluteInt(),
                Y = player.Position.Y.ToAbsoluteInt(),
                Z = player.Position.Z.ToAbsoluteInt(),
                Yaw = MinecraftMath.RotationFloatToSByte(player.Yaw),
                Pitch = MinecraftMath.RotationFloatToSByte(player.Pitch)
            }, remoteClient, readyOnly: true);
            player.PositionDirty = false;
        }

        await World.ProcessAsync(elapsed);
        await EntityManager.ProcessAsync(elapsed);
    }

    public async Task DisconnectAllPlayersAsync(string message)
    {
        await BroadcastPacketAsync(new PlayerDisconnectPacket
        {
            Reason = message
        });
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
            MobType.Creeper => new Creeper(),
            MobType.Skeleton => new Skeleton(),
            MobType.Spider => new Spider(),
            MobType.GiantZombie => new GiantZombie(),
            MobType.Zombie => new Zombie(),
            MobType.Slime => new Slime(),
            MobType.Ghast => new Ghast(),
            MobType.ZombiePigman => new ZombiePigman(),
            MobType.Pig => new Pig(),
            MobType.Sheep => new Sheep(),
            MobType.Cow => new Cow(),
            MobType.Chicken => new Chicken(),
            MobType.Squid => new Squid(),
            MobType.Wolf => new Wolf(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        return await SpawnMobAsync(mob, position);
    }

    public async Task<T> SpawnMobAsync<T>(Vector3i position) where T : IMobEntity, new()
    {
        var mob = new T();
        await SpawnMobAsync(mob, position);
        return mob;
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
            X = mob.Position.X.ToAbsoluteInt(),
            Y = mob.Position.Y.ToAbsoluteInt(),
            Z = mob.Position.Z.ToAbsoluteInt(),
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
            if (_remoteClients.Count >= Configuration.MaxPlayers)
            {
                await remoteClient.SendPacketAsync(new PlayerDisconnectPacket
                {
                    Reason = $"Server is full: {_remoteClients.Count}/{Configuration.MaxPlayers}"
                });
                await remoteClient.DisconnectSocketAsync();
                _logger.LogInformation("Client {networkId} tried to connect but the server is full", remoteClient.NetworkId);
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
            if (remoteClient.Username is not null)
                _logger.LogInformation("Player {username} with network id: {networkId}", remoteClient.Username, remoteClient.NetworkId);
            _logger.LogInformation("Client disconnected with network id: {networkId}", remoteClient.NetworkId);
            await RemoveRemoteClientAsync(remoteClient);
        }
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
            var leftMessage = $"{ChatColors.Blue}{remoteClient.Username} {ChatColors.White}has left the server!";
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
        _addClientSemaphore.Dispose();
    }
}
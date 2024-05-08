using System.Collections.Concurrent;
using System.Diagnostics;
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

public class MinecraftServer
{
    private const int ClientTimeoutInSeconds = 10;

    public IEnumerable<MinecraftRemoteClient> RemoteClients => _remoteClients.Values;
    private readonly ConcurrentDictionary<string, MinecraftRemoteClient> _remoteClients;

    private readonly Socket _socket;
    private readonly PacketDispatcher _packetDispatcher;
    public ServerConfiguration Configuration { get; }
    private readonly ILogger<MinecraftServer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly CommandHandler _commandHandler;

    public MinecraftWorld World { get; }
    public EntityManager EntityManager { get; }
    public Scheduler Scheduler { get; }

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
        _remoteClients = new ConcurrentDictionary<string, MinecraftRemoteClient>();

        World = new MinecraftWorld(this, 42);
        World.GenerateInitialChunks();

        EntityManager = new EntityManager(this);

        Scheduler = new Scheduler();

        RegisterDefaultCommands();

        var ip = new IPEndPoint(IPAddress.Any, configuration.Value.Port);
        _socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _socket.Bind(ip);
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
        
        _commandHandler.TryRegisterCommand("pos", async (server, client, args) =>
        {
            await client!.SendMessageAsync(client.Player!.Position.ToString());
            return true;
        });
        
        _commandHandler.TryRegisterCommand("chunk", async (server, client, args) =>
        {
            await client!.SendMessageAsync(client.GetCurrentChunk().ToString());
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
            await server.BroadcastMessageAsync($"Yaw: {client!.Player!.Yaw}");
            return true;
        });

        _commandHandler.TryRegisterCommand("mob", async (server, client, args) =>
        {
            await server.SpawnMobAsync((MobType) byte.Parse(args[0]), client!.Player!.Position.ToVector3i());
            return true;
        });

        _commandHandler.TryRegisterCommand("test", async (server, client, args) =>
        {
            client!.GetVisibleChunks();
            return true;
        });
    }

    public ILogger<T> GetLogger<T>()
    {
        return _serviceProvider.GetRequiredService<ILogger<T>>();
    }

    public void Start(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Server starting...");

        World.Start();
        Scheduler.Start();

        // Main tick loop
        RegisterLoop(TimeSpan.FromMilliseconds(50), ProcessAsync, cancellationToken);

        RegisterLoop(TimeSpan.FromSeconds(5), SendKeepAlivePacketsAsync, cancellationToken);
        RegisterLoop(TimeSpan.FromSeconds(1), World.SendTimeUpdateAsync, cancellationToken);
        RegisterLoop(TimeSpan.FromSeconds(1), async () =>
        {
            await Parallel.ForEachAsync(RemoteClients, cancellationToken, async (client, token) =>
            {
                if (client.Player is not null)
                    await client.UpdateLoadedChunksAsync();
            });
        }, cancellationToken);

        _socket.Listen();
        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var socket = await _socket.AcceptAsync(cancellationToken);
                _ = HandleClientAsync(socket, cancellationToken);
            }
        }, cancellationToken);

        _logger.LogInformation("Server started on port: {0}", Configuration.Port);
    }

    private void RegisterLoop(TimeSpan interval, Func<Task> func, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(interval);
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await func();
            }
        }, cancellationToken);
    }

    private void RegisterLoop(TimeSpan interval, Func<TimeSpan, Task> func, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            var stopwatch = new Stopwatch();
            using var timer = new PeriodicTimer(interval);
            stopwatch.Start();
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                var elapsed = stopwatch.Elapsed;
                stopwatch.Restart();
                await func(elapsed);
            }
        }, cancellationToken);
    }

    private async Task SendKeepAlivePacketsAsync()
    {
        await BroadcastPacketAsync(new KeepAlivePacket(), readyOnly: true);
    }

    public async Task StopAsync()
    {
        await DisconnectAllPlayersAsync("Server stopped.");

        Scheduler.Stop();
        World.Stop();

        _logger.LogInformation("Server stoping...");
        //_socket.Shutdown(SocketShutdown.Both);
        //_socket.Close();
        _socket.Dispose();
        _logger.LogInformation("Server stopped");
    }

    private async Task ProcessAsync(TimeSpan elapsed)
    {
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
        await Scheduler.ProcessAsync();
    }

    public async Task DisconnectAllPlayersAsync(string message)
    {
        await BroadcastPacketAsync(new PlayerDisconnectPacket
        {
            Reason = message
        });
    }

    public async Task BroadcastPacketAsync(IServerPacket packet, MinecraftRemoteClient? except = null, bool readyOnly = false)
    {
        await using var writer = new PacketWriter();
        packet.Write(writer);
        var data = writer.ToByteArray();
        await Parallel.ForEachAsync(RemoteClients, async (client, _) =>
        {
            if (client == except || (readyOnly && client.State != MinecraftRemoteClient.ClientState.Ready))
                return;
            await client.SendAsync(data);
        });
    }

    public async Task BroadcastMessageAsync(string message, MinecraftRemoteClient? except = null)
    {
        await BroadcastPacketAsync(new ChatMessagePacket
        {
            Message = message
        }, except);
    }

    public async Task<IMobEntity> SpawnMobAsync(MobType type, Vector3i position)
    {
        var mob = await (type switch
        {
            MobType.Creeper => SpawnMobAsync(new Creeper(), position),
            MobType.Skeleton => SpawnMobAsync(new Skeleton(), position),
            MobType.Spider => SpawnMobAsync(new Spider(), position),
            MobType.GiantZombie => SpawnMobAsync(new GiantZombie(), position),
            MobType.Zombie => SpawnMobAsync(new Zombie(), position),
            MobType.Slime => SpawnMobAsync(new Slime(), position),
            MobType.Ghast => SpawnMobAsync(new Ghast(), position),
            MobType.ZombiePigman => SpawnMobAsync(new ZombiePigman(), position),
            MobType.Pig => SpawnMobAsync(new Pig(), position),
            MobType.Sheep => SpawnMobAsync(new Sheep(), position),
            MobType.Cow => SpawnMobAsync(new Cow(), position),
            MobType.Chicken => SpawnMobAsync(new Chicken(), position),
            MobType.Squid => SpawnMobAsync(new Squid(), position),
            MobType.Wolf => SpawnMobAsync(new Wolf(), position),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        });
        return mob;
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
        var remoteClient = new MinecraftRemoteClient(socket, this);

        if (_remoteClients.Count >= Configuration.MaxPlayers)
        {
            await remoteClient.SendPacketAsync(new PlayerDisconnectPacket
            {
                Reason = $"Server is full: {_remoteClients.Count}/{Configuration.MaxPlayers}"
            });
            await remoteClient.DisconnectSocketAsync();
            return;
        }

        AddRemoteClient(remoteClient);

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
            _logger.LogError("Exception({0}): {1}", ex.GetType().ToString(), ex.Message);
            if (!socket.Connected)
                throw;
        }
        finally
        {
            _logger.LogInformation("Client disconnected with network id: {0}", remoteClient.NetworkId);
            await RemoveRemoteClientAsync(remoteClient);
        }
    }

    private void AddRemoteClient(MinecraftRemoteClient remoteClient)
    {
        if (!_remoteClients.TryAdd(remoteClient.NetworkId, remoteClient))
            throw new Exception("Failed to add client to list");

        _logger.LogInformation("Client connected with network id: {0}", remoteClient.NetworkId);
    }

    private async Task RemoveRemoteClientAsync(MinecraftRemoteClient remoteClient)
    {
        if (!_remoteClients.Remove(remoteClient.NetworkId, out _))
            throw new Exception("Failed to remove client");
        remoteClient.Dispose();

        await PlayerDisconnectedAsync(remoteClient);
    }

    private async Task PlayerDisconnectedAsync(MinecraftRemoteClient remoteClient)
    {
        if (remoteClient.Player is not null)
        {
            var leftMessage = $"{ChatColors.Blue}{remoteClient.Username} {ChatColors.White}has left the server!";
            await BroadcastMessageAsync(leftMessage, remoteClient);

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
                var task = _packetDispatcher.DispatchPacket(context, buffer, out var position);
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to handle packet");
                }
                finally
                {
                    // Tell the PipeReader how much of the buffer has been consumed.
                    pipe.Reader.AdvanceTo(position);
                }
            }

            // Stop reading if there's no more data coming.
            if (result.IsCompleted)
                break;
        }

        // Mark the PipeReader as complete.
        await pipe.Reader.CompleteAsync();
    }
}
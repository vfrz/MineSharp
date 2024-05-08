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
using MineSharp.Network;
using MineSharp.Network.Packets;
using MineSharp.World;

namespace MineSharp.Core;

public class MinecraftServer
{
    public IEnumerable<MinecraftRemoteClient> RemoteClients => _remoteClients.Values;
    private readonly ConcurrentDictionary<string, MinecraftRemoteClient> _remoteClients;

    private readonly Socket _socket;
    private readonly PacketDispatcher _packetDispatcher;
    private readonly IOptions<ServerConfiguration> _configuration;
    private readonly ILogger<MinecraftServer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly CommandHandler _commandHandler;

    public MinecraftWorld World { get; }
    public EntityManager EntityManager { get; }

    public MinecraftServer(PacketDispatcher packetDispatcher,
        IOptions<ServerConfiguration> configuration,
        ILogger<MinecraftServer> logger,
        IServiceProvider serviceProvider,
        CommandHandler commandHandler)
    {
        _packetDispatcher = packetDispatcher;
        _configuration = configuration;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _commandHandler = commandHandler;
        _remoteClients = new ConcurrentDictionary<string, MinecraftRemoteClient>();

        World = new MinecraftWorld(this);
        World.InitializeDefault();

        EntityManager = new EntityManager();
        
        RegisterDefaultCommands();

        var ip = new IPEndPoint(IPAddress.Any, configuration.Value.Port);
        _socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _socket.Bind(ip);
    }

    private void RegisterDefaultCommands()
    {
        _commandHandler.RegisterCommand("id", async (server, client, args) =>
        {
            if (client is null)
                return true;
            await client.SendPacketAsync(new ChatMessagePacket
            {
                Message = $"Your entity id: {client.Player!.EntityId}"
            });
            return true;
        });
        
        _commandHandler.RegisterCommand("rain", async (server, client, args) =>
        {
            if (server.World.Raining)
                await server.World.StopRainAsync();
            else
                await server.World.StartRainAsync();
            return true;
        });
        
        _commandHandler.RegisterCommand("time", async (server, client, args) =>
        {
            var time = long.Parse(args[0]);
            await server.World.SetTimeAsync(time);
            return true;
        });
        
        _commandHandler.RegisterCommand("heal", async (server, client, args) =>
        {
            await client!.Player!.SetHealthAsync(20);
            return true;
        });
        
        _commandHandler.RegisterCommand("kill", async (server, client, args) =>
        {
            await client!.Player!.KillAsync();
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

        _socket.Listen();

        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var socket = await _socket.AcceptAsync(cancellationToken);
                _ = HandleClientAsync(socket, cancellationToken);
            }
        }, cancellationToken);

        World.Start();

        // Main tick loop
        RegisterLoop(TimeSpan.FromMilliseconds(50), ProcessAsync, cancellationToken);

        RegisterLoop(TimeSpan.FromSeconds(5), SendKeepAlivePacketsAsync, cancellationToken);
        RegisterLoop(TimeSpan.FromSeconds(1), World.SendTimeUpdateAsync, cancellationToken);

        _logger.LogInformation("Server started on port: {0}", _configuration.Value.Port);
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

            if (player.Y < -50)
                await player.KillAsync();

            await BroadcastPacketAsync(new EntityTeleportPacket
            {
                EntityId = player.EntityId,
                X = (int) (player.X * 32),
                Y = (int) (player.Y * 32),
                Z = (int) (player.Z * 32),
                Yaw = MinecraftMath.RotationFloatToSByte(player.Yaw),
                Pitch = MinecraftMath.RotationFloatToSByte(player.Pitch)
            }, remoteClient, readyOnly: true);
            player.PositionDirty = false;
        }

        await World.ProcessAsync(elapsed);
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

    private async Task HandleClientAsync(Socket socket, CancellationToken cancellationToken)
    {
        var remoteClient = new MinecraftRemoteClient(socket, this);

        if (_remoteClients.Count >= _configuration.Value.MaxPlayers)
        {
            await remoteClient.SendPacketAsync(new PlayerDisconnectPacket
            {
                Reason = $"Server is full: {_remoteClients.Count}/{_configuration.Value.MaxPlayers}"
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
        await BroadcastMessageAsync($"{ChatColors.Blue}{remoteClient.Username} {ChatColors.White}has left the server!", remoteClient);

        if (remoteClient.Player is not null)
        {
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
            // Allocate at least 65535 bytes from the PipeWriter. (Maximum tcp packet size)
            var memory = pipe.Writer.GetMemory(65535);

            var bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
            if (bytesRead == 0)
                break;

            // Tell the PipeWriter how much was read from the Socket.
            pipe.Writer.Advance(bytesRead);

            // Make the data available to the PipeReader.
            var result = await pipe.Writer.FlushAsync(cancellationToken);

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
            var result = await pipe.Reader.ReadAsync(cancellationToken);
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
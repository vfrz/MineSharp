using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MineSharp.Configuration;
using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Packets;

namespace MineSharp.Network;

public class MinecraftServer
{
    public IEnumerable<MinecraftRemoteClient> RemoteClients => _remoteClients.Values;
    private readonly ConcurrentDictionary<string, MinecraftRemoteClient> _remoteClients;

    private readonly Socket _listener;
    private readonly PacketsHandler _packetsHandler;
    private readonly IOptions<ServerConfiguration> _configuration;
    private readonly ILogger<MinecraftServer> _logger;
    private readonly EntityIdGenerator _entityIdGenerator;

    public World World { get; }

    public MinecraftServer(PacketsHandler packetsHandler,
        IOptions<ServerConfiguration> configuration,
        ILogger<MinecraftServer> logger,
        EntityIdGenerator entityIdGenerator)
    {
        _packetsHandler = packetsHandler;
        _configuration = configuration;
        _logger = logger;
        _entityIdGenerator = entityIdGenerator;
        _remoteClients = new ConcurrentDictionary<string, MinecraftRemoteClient>();

        World = new World();
        World.InitializeDefault();

        var ip = new IPEndPoint(IPAddress.Any, configuration.Value.Port);
        _listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _listener.Bind(ip);
    }

    public void Start(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Server starting...");

        _listener.Listen();

        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var socket = await _listener.AcceptAsync(cancellationToken);
                _ = HandleClientAsync(socket, cancellationToken);
            }
        }, cancellationToken);

        Task.Run(async () =>
        {
            var stopwatch = new Stopwatch();
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(1 / 20d));
            stopwatch.Start();
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                var elapsed = stopwatch.Elapsed;
                stopwatch.Restart();
                await UpdateAsync(elapsed);
            }
        }, cancellationToken);

        Task.Run(async () =>
        {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await BroadcastPacketAsync(new KeepAlivePacket());
            }
        }, cancellationToken);

        _logger.LogInformation("Server started on port: {0}", _configuration.Value.Port);
    }

    public async Task StopAsync()
    {
        await DisconnectAllPlayersAsync("Server stopped.");

        _logger.LogInformation("Server stoping...");
        _listener.Shutdown(SocketShutdown.Both);
        _listener.Close();
        _listener.Dispose();
        _logger.LogInformation("Server stopped");
    }

    public async Task UpdateAsync(TimeSpan elapsed)
    {
        foreach (var remoteClient in RemoteClients)
        {
            var player = remoteClient.Player;
            if (player is null || player.PositionDirty is false)
                continue;
            await BroadcastPacketAsync(new EntityTeleportPacket
            {
                EntityId = player.EntityId,
                X = (int) (player.X * 32),
                Y = (int) (player.Y * 32),
                Z = (int) (player.Z * 32),
                Yaw = MinecraftMath.RotationFloatToSByte(player.Yaw),
                Pitch = MinecraftMath.RotationFloatToSByte(player.Pitch)
            }, remoteClient);
            player.PositionDirty = false;
        }
    }

    public async Task DisconnectAllPlayersAsync(string message)
    {
        await BroadcastPacketAsync(new PlayerDisconnectPacket
        {
            Reason = message
        });
    }

    public async Task BroadcastPacketAsync(IServerPacket packet, MinecraftRemoteClient? except = null)
    {
        await using var writer = new PacketWriter();
        packet.Write(writer);
        var data = writer.ToByteArray();
        await Parallel.ForEachAsync(RemoteClients, async (client, _) =>
        {
            if (client == except)
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
        var remoteClient = new MinecraftRemoteClient(socket);

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
        await BroadcastMessageAsync($"{ChatColors.Blue}{remoteClient.Username} {ChatColors.White}has left the server!", remoteClient);


        if (remoteClient.Player is not null)
        {
            await BroadcastPacketAsync(new DestroyEntityPacket
            {
                EntityId = remoteClient.Player.EntityId
            }, remoteClient);

            _entityIdGenerator.Release(remoteClient.Player.EntityId);
        }

        _remoteClients.Remove(remoteClient.NetworkId, out _);
        remoteClient.Dispose();
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
                var task = _packetsHandler.HandlePacket(context, buffer, out var position);
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
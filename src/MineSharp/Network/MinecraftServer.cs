using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MineSharp.Configuration;
using MineSharp.Core;
using MineSharp.Extensions;
using MineSharp.Notifications;

namespace MineSharp.Network;

public class MinecraftServer
{
    public IEnumerable<MinecraftClient> Clients => _clients.Values;
    private readonly ConcurrentDictionary<string, MinecraftClient> _clients;

    private readonly Socket _listener;
    private readonly IMediator _mediator;
    private readonly PacketHandler _packetHandler;
    private readonly IOptions<ServerConfiguration> _configuration;
    private readonly ILogger<MinecraftServer> _logger;

    public string? FaviconBase64 { get; private set; }

    public MinecraftServer(IMediator mediator, PacketHandler packetHandler, IOptions<ServerConfiguration> configuration,
        ILogger<MinecraftServer> logger)
    {
        _mediator = mediator;
        _packetHandler = packetHandler;
        _configuration = configuration;
        _logger = logger;
        _clients = new ConcurrentDictionary<string, MinecraftClient>();
        var ip = new IPEndPoint(IPAddress.Any, configuration.Value.Port);
        _listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _listener.Bind(ip);
    }

    public void Start(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Server starting...");

        if (File.Exists("icon.png"))
        {
            FaviconBase64 = Convert.ToBase64String(File.ReadAllBytes("icon.png"));
        }

        _listener.Listen();

        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var socket = await _listener.AcceptAsync(cancellationToken);
                var _ = HandleClientAsync(socket, cancellationToken);
            }
        }, cancellationToken);

        _logger.LogInformation("Server started on port: {0}", _configuration.Value.Port);
    }

    public void Stop()
    {
        _logger.LogInformation("Server stoping...");
        _listener.Shutdown(SocketShutdown.Both);
        _listener.Close();
        _listener.Dispose();
        _logger.LogInformation("Server stopped");
    }

    private async Task HandleClientAsync(Socket socket, CancellationToken cancellationToken)
    {
        var client = await CreateAndRegisterMinecraftClientAsync(socket);

        var pipe = new Pipe();

        try
        {
            var writing = FillPipeAsync(client, pipe, cancellationToken);
            var reading = ReadPipeAsync(client, pipe, cancellationToken);

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
            _logger.LogInformation("Client (socket) disconnected with network id: {0}", client.NetworkId);
            _clients.Remove(client.NetworkId, out _);
            client.Dispose();
        }
    }

    private async Task<MinecraftClient> CreateAndRegisterMinecraftClientAsync(Socket socket)
    {
        var client = new MinecraftClient(socket);
        if (!_clients.TryAdd(client.NetworkId, client))
            throw new Exception("Failed to add client to list");
        
        _logger.LogInformation("Client (socket) connected with network id: {0}", client.NetworkId);
        await _mediator.Publish(new MinecraftClientConnected(client));

        return client;
    }
    
    private async Task FillPipeAsync(MinecraftClient client, Pipe pipe, CancellationToken cancellationToken)
    {
        const int minimumBufferSize = 512;

        while (true)
        {
            // Allocate at least 512 bytes from the PipeWriter.
            var memory = pipe.Writer.GetMemory(minimumBufferSize);

            var bytesRead = await client.SocketWrapper.Socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
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

    private async Task ReadPipeAsync(MinecraftClient client, Pipe pipe, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await pipe.Reader.ReadAsync(cancellationToken);
            var buffer = result.Buffer;

            while (TryReadPacket(ref buffer, out var packet))
            {
                _packetHandler.HandlePacket(client, packet);
            }

            // Tell the PipeReader how much of the buffer has been consumed.
            pipe.Reader.AdvanceTo(buffer.Start, buffer.End);

            // Stop reading if there's no more data coming.
            if (result.IsCompleted)
            {
                break;
            }
        }

        // Mark the PipeReader as complete.
        await pipe.Reader.CompleteAsync();
    }

    private bool TryReadPacket(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> packet)
    {
        var reader = new SequenceReader<byte>(buffer);

        if (!reader.TryReadVarInt(out var packetSize, out var packetSizeBytesRead)
            || buffer.Length < packetSizeBytesRead + packetSize)
        {
            packet = default;
            return false;
        }

        packet = buffer.Slice(0, packetSizeBytesRead + packetSize);
        buffer = buffer.Slice(packetSizeBytesRead + packetSize);
        return true;
    }
}
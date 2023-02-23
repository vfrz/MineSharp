using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MineSharp.Commands;
using MineSharp.Core;
using MineSharp.Extensions;
using MineSharp.Notifications;

namespace MineSharp.Network;

public class MinecraftServer
{
    public IReadOnlyList<MinecraftClient> Clients => _clients;
    private readonly List<MinecraftClient> _clients;

    private readonly Socket _listener;
    private readonly IMediator _mediator;
    private readonly PacketHandler _packetHandler;
    private readonly IOptions<ServerConfiguration> _configuration;
    private readonly ILogger<MinecraftServer> _logger;

    public MinecraftServer(IMediator mediator, PacketHandler packetHandler, IOptions<ServerConfiguration> configuration, 
        ILogger<MinecraftServer> logger)
    {
        _mediator = mediator;
        _packetHandler = packetHandler;
        _configuration = configuration;
        _logger = logger;
        _clients = new List<MinecraftClient>();
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
                var socket = await _listener.AcceptAsync(cancellationToken); // await _listener.AcceptSocketAsync(cancellationToken).ConfigureAwait(false);
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
        var client = new MinecraftClient(socket);
        _clients.Add(client);
        await _mediator.Publish(new MinecraftClientConnected(client), cancellationToken);

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
                throw ex;
        }
        finally
        {
            _clients.Remove(client);
            client.Dispose();
        }
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
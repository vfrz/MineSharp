using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Mediator;
using MineSharp.Commands;
using MineSharp.Extensions;
using MineSharp.Notifications;

namespace MineSharp.Network;

public class MinecraftServer
{
    public IReadOnlyList<MinecraftClient> Clients => _clients;
    private readonly List<MinecraftClient> _clients;

    private readonly TcpListener _listener;
    private readonly IMediator _mediator;

    public MinecraftServer(IMediator mediator)
    {
        _mediator = mediator;
        _clients = new List<MinecraftClient>();
        _listener = new TcpListener(IPAddress.Any, 25565);
    }

    public void Start(CancellationToken cancellationToken)
    {
        _listener.Start();

        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var socket = await _listener.AcceptSocketAsync(cancellationToken).ConfigureAwait(false);
                var _ = HandleClientAsync(socket, cancellationToken);
            }
        }, cancellationToken);
    }

    public void Stop()
    {
        _listener.Stop();
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
            Console.WriteLine($"Exception: {ex.Message}");
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
                HandlePacket(client, packet);
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

    private void HandlePacket(MinecraftClient client, ReadOnlySequence<byte> packet)
    {
        var packetReader = new SequenceReader<byte>(packet);
        packetReader.TryReadVarInt(out var packetSize);
        packetReader.TryReadVarInt(out var packetId, out var packetIdBytesRead);

        var dataSize = packetSize - packetIdBytesRead;

        switch (packetId)
        {
            case 0:
                HandleStatusRequestPacket(ref packetReader, client, dataSize);
                break;
            case 1:
                HandlePingRequestPacket(ref packetReader, client);
                break;
            default:
                HandleUnknownPacket(ref packetReader, client, packetId, dataSize);
                break;
        }
    }

    private void HandleStatusRequestPacket(ref SequenceReader<byte> reader, MinecraftClient client, int dataSize)
    {
        if (dataSize > 0)
        {
            var protocolVersion = reader.ReadVarInt();
            var serverAddress = reader.ReadString();
            var serverPort = reader.ReadUInt16();
            var nextState = reader.ReadVarInt();
            _mediator.Send(new ClientHandshake(client, protocolVersion, serverAddress, serverPort, nextState));
        }
        else
        {
            _mediator.Send(new StatusRequest(client));
        }
    }

    private void HandlePingRequestPacket(ref SequenceReader<byte> reader, MinecraftClient client)
    {
        var payload = reader.ReadLong();
        _mediator.Send(new PingRequest(client, payload));
    }

    private void HandleUnknownPacket(ref SequenceReader<byte> reader, MinecraftClient client, int packetId, int dataSize)
    {
        var packetData = dataSize > 0 ? reader.ReadBytesArray(dataSize) : Array.Empty<byte>();
        _mediator.Send(new UnknownPacket(client, packetId, packetData));
    }
}
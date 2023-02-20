using System.Buffers;
using System.IO.Pipelines;
using Mediator;
using MineSharp.Commands;
using MineSharp.Extensions;

namespace MineSharp.Network;

public class PacketHandler
{
    private readonly IMediator _mediator;
    
    public PacketHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task HandlePacketAsync(MinecraftClient client, ReadOnlySequence<byte> packet)
    {
    }
    
    public void HandlePacket(MinecraftClient client, ReadOnlySequence<byte> packet)
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
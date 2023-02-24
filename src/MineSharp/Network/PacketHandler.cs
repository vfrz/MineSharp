using System.Buffers;
using Mediator;
using MineSharp.Core;
using MineSharp.Extensions;
using MineSharp.Packets;

namespace MineSharp.Network;

public class PacketHandler
{
    private readonly IMediator _mediator;

    public PacketHandler(IMediator mediator)
    {
        _mediator = mediator;
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
                HandleStatusOrLoginRequestPacket(ref packetReader, client, dataSize);
                break;
            case 1:
                HandlePingRequestPacket(ref packetReader, client);
                break;
            default:
                HandleUnknownPacket(ref packetReader, client, packetId, dataSize);
                break;
        }
    }

    private void HandleStatusOrLoginRequestPacket(ref SequenceReader<byte> reader, MinecraftClient client, int dataSize)
    {
        if (client.State is MinecraftClientState.Default)
        {
            var protocolVersion = reader.ReadVarInt();
            var serverAddress = reader.ReadString();
            var serverPort = reader.ReadUInt16();
            var nextState = (MinecraftClientState) reader.ReadVarInt();
            _mediator.Send(new ClientHandshake(client, protocolVersion, serverAddress, serverPort, nextState));
            return;
        }
        
        if (client.State is MinecraftClientState.Status)
        {
            _mediator.Send(new StatusRequest(client));
            return;
        }

        if (client.State is MinecraftClientState.Login)
        {
            var name = reader.ReadString();
            var hasUniqueId = reader.ReadBool();
            Guid? uniqueId = null;
            if (hasUniqueId)
                uniqueId = reader.ReadGuid();
            _mediator.Send(new LoginStart(client, name, hasUniqueId, uniqueId));
            return;
        }

        throw new Exception($"Client state is invalid: {client.State}");
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
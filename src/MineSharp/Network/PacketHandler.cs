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
        var reader = new SequenceReader<byte>(packet);
        var packetId = reader.ReadByte();
        switch (packetId)
        {
            case 0x01:
                HandleLoginRequestPacket(ref reader, client);
                break;
            case 0x02:
                HandleHandshakeRequestPacket(ref reader, client);
                break;
            case 0x03:
                HandleChatMessagePacket(ref reader, client);
                break;
            case 0x0E:
                HandlePlayerDigging(ref reader, client);
                break;
            case 0x0F:
                HandlePlayerBlockPlacement(ref reader, client);
                break;
            case 0xFF:
                HandlePlayerDisconnect(client);
                break;
            default:
                HandleUnknownPacket(ref reader, client, packetId);
                break;
        }
    }

    private void HandleHandshakeRequestPacket(ref SequenceReader<byte> reader, MinecraftClient client)
    {
        var username = reader.ReadString();
        _mediator.Send(new HandshakeRequest(client, username, MinecraftClientState.Handshake));
    }

    private void HandleLoginRequestPacket(ref SequenceReader<byte> reader, MinecraftClient client)
    {
        var protocolVersion = reader.ReadInt();
        var username = reader.ReadString();
        // Unused mapSeed and dimension
        reader.ReadLong();
        reader.ReadByte();
        _mediator.Send(new LoginRequest(client, protocolVersion, username));
    }

    private void HandleChatMessagePacket(ref SequenceReader<byte> reader, MinecraftClient client)
    {
        var message = reader.ReadString();
        _mediator.Send(new ChatMessage(client, message));
    }

    private void HandlePlayerBlockPlacement(ref SequenceReader<byte> reader, MinecraftClient client)
    {
        var x = reader.ReadInt();
        var y = reader.ReadSByte();
        var z = reader.ReadInt();
        var direction = reader.ReadSByte();
        var blockId = reader.ReadShort();
        byte? amount = blockId != -1 ? reader.ReadByte() : null;
        short? damage = blockId != -1 ? reader.ReadShort() : null;
        _mediator.Send(new PlayerBlockPlacement(client, x, y, z, direction, blockId, amount, damage));
    }

    private void HandlePlayerDigging(ref SequenceReader<byte> reader, MinecraftClient client)
    {
        var status = (PlayerDiggingStatus) reader.ReadSByte();
        var x = reader.ReadInt();
        var y = reader.ReadSByte();
        var z = reader.ReadInt();
        var face = reader.ReadSByte();
        _mediator.Send(new PlayerDigging(client, status, x, y, z, face));
    }

    private void HandlePlayerDisconnect(MinecraftClient client)
    {
        //TODO this is ugly but for testing
        client.DisconnectAsync().GetAwaiter().GetResult();
    }
    
    private void HandleUnknownPacket(ref SequenceReader<byte> reader, MinecraftClient client, int packetId)
    {
        var packetLength = (int) reader.Length - 1;
        var packetData = packetLength > 0 ? reader.ReadBytesArray(packetLength) : Array.Empty<byte>();
        _mediator.Send(new UnknownPacket(client, packetId, packetData));
    }
}
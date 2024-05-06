using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;
using MineSharp.Packets;

namespace MineSharp.Network;

public class PacketsHandler
{
    private readonly IClientPacketHandler<HandshakeRequestPacket> _handshakeRequestPacketHandler;
    private readonly IClientPacketHandler<ChatMessagePacket> _chatMessagePacketHandler;
    private readonly IClientPacketHandler<LoginRequestPacket> _loginRequestPacketHandler;
    private readonly IClientPacketHandler<PlayerDiggingPacket> _playerDiggingPacketHandler;
    private readonly IClientPacketHandler<PlayerBlockPlacementPacket> _playerBlockPlacementPacketHandler;
    private readonly IClientPacketHandler<PlayerDisconnectPacket> _playerDisconnectPacketHandler;

    public PacketsHandler(IClientPacketHandler<HandshakeRequestPacket> handshakeRequestPacketHandler,
        IClientPacketHandler<ChatMessagePacket> chatMessagePacketHandler,
        IClientPacketHandler<LoginRequestPacket> loginRequestPacketHandler,
        IClientPacketHandler<PlayerDiggingPacket> playerDiggingPacketHandler,
        IClientPacketHandler<PlayerBlockPlacementPacket> playerBlockPlacementPacketHandler,
        IClientPacketHandler<PlayerDisconnectPacket> playerDisconnectPacketHandler)
    {
        _handshakeRequestPacketHandler = handshakeRequestPacketHandler;
        _chatMessagePacketHandler = chatMessagePacketHandler;
        _loginRequestPacketHandler = loginRequestPacketHandler;
        _playerDiggingPacketHandler = playerDiggingPacketHandler;
        _playerBlockPlacementPacketHandler = playerBlockPlacementPacketHandler;
        _playerDisconnectPacketHandler = playerDisconnectPacketHandler;
    }

    public Task HandlePacket(ClientPacketHandlerContext context, ReadOnlySequence<byte> buffer, out SequencePosition finalPosition)
    {
        var reader = new SequenceReader<byte>(buffer);
        var packetId = reader.ReadByte();

        var task = packetId switch
        {
            LoginRequestPacket.Id => HandleLoginRequestPacket(ref reader, context),
            HandshakeRequestPacket.Id => HandleHandshakeRequestPacket(ref reader, context),
            ChatMessagePacket.Id => HandleChatMessagePacket(ref reader, context),
            PlayerDiggingPacket.Id => HandlePlayerDiggingPacket(ref reader, context),
            PlayerBlockPlacementPacket.Id => HandlePlayerBlockPlacementPacket(ref reader, context),
            PlayerDisconnectPacket.Id => HandlePlayerDisconnectPacket(ref reader, context),
            _ => HandleUnknownPacket(ref reader, context, packetId)
        };

        finalPosition = reader.Position;

        return task;
    }

    private Task HandleLoginRequestPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new LoginRequestPacket();
        packet.Read(ref reader);
        return _loginRequestPacketHandler.HandleAsync(packet, context);
    }

    private Task HandleHandshakeRequestPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new HandshakeRequestPacket();
        packet.Read(ref reader);
        return _handshakeRequestPacketHandler.HandleAsync(packet, context);
    }


    private Task HandleChatMessagePacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new ChatMessagePacket();
        packet.Read(ref reader);
        return _chatMessagePacketHandler.HandleAsync(packet, context);
    }

    private Task HandlePlayerDiggingPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerDiggingPacket();
        packet.Read(ref reader);
        return _playerDiggingPacketHandler.HandleAsync(packet, context);
    }

    private Task HandlePlayerBlockPlacementPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerBlockPlacementPacket();
        packet.Read(ref reader);
        return _playerBlockPlacementPacketHandler.HandleAsync(packet, context);
    }

    private Task HandlePlayerDisconnectPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerDisconnectPacket();
        packet.Read(ref reader);
        return _playerDisconnectPacketHandler.HandleAsync(packet, context);
    }

    private Task HandleUnknownPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context, int packetId)
    {
        //todo temporary hack to avoid packet loss
        if (packetId == 0x0A)
        {
            reader.Advance(1);
            return Task.CompletedTask;
        }

        if (packetId == 0x0B)
        {
            reader.Advance(33);
            return Task.CompletedTask;
        }

        if (packetId == 0x0C)
        {
            reader.Advance(9);
            return Task.CompletedTask;
        }

        if (packetId == 0x0D)
        {
            reader.Advance(41);
            return Task.CompletedTask;
        }

        if (packetId == 0x10)
        {
            reader.Advance(2);
            return Task.CompletedTask;
        }

        if (packetId == 0x12)
        {
            reader.Advance(5);
            return Task.CompletedTask;
        }

        if (packetId == 0x13)
        {
            reader.Advance(5);
            return Task.CompletedTask;
        }

        var packetSize = (int) reader.Length - 1;
        //TODO Dangerous because we can lose data
        var packetData = packetSize > 0 ? reader.ReadBytesArray(packetSize) : Array.Empty<byte>();
        Console.WriteLine($"Unknown packet: 0x{packetId:X} with data length: {packetData.Length}");
        return Task.CompletedTask;
    }
}
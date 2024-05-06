using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;
using MineSharp.Packets;

namespace MineSharp.Network;

public class PacketsHandler
{
    private readonly IClientPacketHandler<ChatMessagePacket> _chatMessagePacketHandler;
    private readonly IClientPacketHandler<HandshakeRequestPacket> _handshakeRequestPacketHandler;
    private readonly IClientPacketHandler<LoginRequestPacket> _loginRequestPacketHandler;
    private readonly IClientPacketHandler<PlayerBlockPlacementPacket> _playerBlockPlacementPacketHandler;
    private readonly IClientPacketHandler<PlayerDiggingPacket> _playerDiggingPacketHandler;
    private readonly IClientPacketHandler<PlayerDisconnectPacket> _playerDisconnectPacketHandler;
    private readonly IClientPacketHandler<PlayerLookPacket> _playerLookPacketHandler;
    private readonly IClientPacketHandler<PlayerPacket> _playerPacketHandler;
    private readonly IClientPacketHandler<PlayerPositionAndLookClientPacket> _playerPositionAndLookClientPacketHandler;
    private readonly IClientPacketHandler<PlayerPositionPacket> _playerPositionPacketHandler;

    public PacketsHandler(IClientPacketHandler<ChatMessagePacket> chatMessagePacketHandler,
        IClientPacketHandler<HandshakeRequestPacket> handshakeRequestPacketHandler,
        IClientPacketHandler<LoginRequestPacket> loginRequestPacketHandler,
        IClientPacketHandler<PlayerBlockPlacementPacket> playerBlockPlacementPacketHandler,
        IClientPacketHandler<PlayerDiggingPacket> playerDiggingPacketHandler,
        IClientPacketHandler<PlayerDisconnectPacket> playerDisconnectPacketHandler,
        IClientPacketHandler<PlayerLookPacket> playerLookPacketHandler,
        IClientPacketHandler<PlayerPacket> playerPacketHandler,
        IClientPacketHandler<PlayerPositionAndLookClientPacket> playerPositionAndLookClientPacketHandler,
        IClientPacketHandler<PlayerPositionPacket> playerPositionPacketHandler)
    {
        _chatMessagePacketHandler = chatMessagePacketHandler;
        _handshakeRequestPacketHandler = handshakeRequestPacketHandler;
        _loginRequestPacketHandler = loginRequestPacketHandler;
        _playerBlockPlacementPacketHandler = playerBlockPlacementPacketHandler;
        _playerDiggingPacketHandler = playerDiggingPacketHandler;
        _playerDisconnectPacketHandler = playerDisconnectPacketHandler;
        _playerLookPacketHandler = playerLookPacketHandler;
        _playerPacketHandler = playerPacketHandler;
        _playerPositionAndLookClientPacketHandler = playerPositionAndLookClientPacketHandler;
        _playerPositionPacketHandler = playerPositionPacketHandler;
    }

    public Task HandlePacket(ClientPacketHandlerContext context, ReadOnlySequence<byte> buffer, out SequencePosition finalPosition)
    {
        var reader = new SequenceReader<byte>(buffer);
        var packetId = reader.ReadByte();

        var task = packetId switch
        {
            ChatMessagePacket.Id => HandleChatMessagePacket(ref reader, context),
            HandshakeRequestPacket.Id => HandleHandshakeRequestPacket(ref reader, context),
            LoginRequestPacket.Id => HandleLoginRequestPacket(ref reader, context),
            PlayerBlockPlacementPacket.Id => HandlePlayerBlockPlacementPacket(ref reader, context),
            PlayerDiggingPacket.Id => HandlePlayerDiggingPacket(ref reader, context),
            PlayerDisconnectPacket.Id => HandlePlayerDisconnectPacket(ref reader, context),
            PlayerLookPacket.Id => HandlePlayerLookPacket(ref reader, context),
            PlayerPacket.Id => HandlePlayerPacket(ref reader, context),
            PlayerPositionAndLookClientPacket.Id => HandlePlayerPositionAndLookClientPacket(ref reader, context),
            PlayerPositionPacket.Id => HandlePlayerPositionPacket(ref reader, context),
            _ => HandleUnknownPacket(ref reader, context, packetId)
        };

        finalPosition = reader.Position;

        return task;
    }

    private Task HandleChatMessagePacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new ChatMessagePacket();
        packet.Read(ref reader);
        return _chatMessagePacketHandler.HandleAsync(packet, context);
    }

    private Task HandleHandshakeRequestPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new HandshakeRequestPacket();
        packet.Read(ref reader);
        return _handshakeRequestPacketHandler.HandleAsync(packet, context);
    }

    private Task HandleLoginRequestPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new LoginRequestPacket();
        packet.Read(ref reader);
        return _loginRequestPacketHandler.HandleAsync(packet, context);
    }

    private Task HandlePlayerBlockPlacementPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerBlockPlacementPacket();
        packet.Read(ref reader);
        return _playerBlockPlacementPacketHandler.HandleAsync(packet, context);
    }

    private Task HandlePlayerDiggingPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerDiggingPacket();
        packet.Read(ref reader);
        return _playerDiggingPacketHandler.HandleAsync(packet, context);
    }

    private Task HandlePlayerDisconnectPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerDisconnectPacket();
        packet.Read(ref reader);
        return _playerDisconnectPacketHandler.HandleAsync(packet, context);
    }

    private Task HandlePlayerLookPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerLookPacket();
        packet.Read(ref reader);
        return _playerLookPacketHandler.HandleAsync(packet, context);
    }

    private Task HandlePlayerPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerPacket();
        packet.Read(ref reader);
        return _playerPacketHandler.HandleAsync(packet, context);
    }

    private Task HandlePlayerPositionAndLookClientPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerPositionAndLookClientPacket();
        packet.Read(ref reader);
        return _playerPositionAndLookClientPacketHandler.HandleAsync(packet, context);
    }

    private Task HandlePlayerPositionPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerPositionPacket();
        packet.Read(ref reader);
        return _playerPositionPacketHandler.HandleAsync(packet, context);
    }

    private Task HandleUnknownPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context, int packetId)
    {
        //todo temporary hack to avoid packet loss
        if (packetId == 0x07)
        {
            reader.Advance(9);
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
using System.Buffers;
using Microsoft.Extensions.DependencyInjection;
using MineSharp.Core.Packets;
using MineSharp.Extensions;
using MineSharp.Packets;

namespace MineSharp.Network;

public class PacketsHandler
{
    private readonly IServiceProvider _serviceProvider;

    public PacketsHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task HandlePacket(ClientPacketHandlerContext context, ReadOnlySequence<byte> buffer, out SequencePosition finalPosition)
    {
        var reader = new SequenceReader<byte>(buffer);
        var packetId = reader.ReadByte();

        var task = packetId switch
        {
            ChatMessagePacket.Id => HandleChatMessagePacket(ref reader, context),
            EntityActionPacket.Id => HandleEntityActionPacket(ref reader, context),
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
        return CallHandlerAsync(packet, context);
    }

    private Task HandleEntityActionPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new EntityActionPacket();
        packet.Read(ref reader);
        return CallHandlerAsync(packet, context);
    }

    private Task HandleHandshakeRequestPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new HandshakeRequestPacket();
        packet.Read(ref reader);
        return CallHandlerAsync(packet, context);
    }

    private Task HandleLoginRequestPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new LoginRequestPacket();
        packet.Read(ref reader);
        return CallHandlerAsync(packet, context);
    }

    private Task HandlePlayerBlockPlacementPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerBlockPlacementPacket();
        packet.Read(ref reader);
        return CallHandlerAsync(packet, context);
    }

    private Task HandlePlayerDiggingPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerDiggingPacket();
        packet.Read(ref reader);
        return CallHandlerAsync(packet, context);
    }

    private Task HandlePlayerDisconnectPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerDisconnectPacket();
        packet.Read(ref reader);
        return CallHandlerAsync(packet, context);
    }

    private Task HandlePlayerLookPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerLookPacket();
        packet.Read(ref reader);
        return CallHandlerAsync(packet, context);
    }

    private Task HandlePlayerPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerPacket();
        packet.Read(ref reader);
        return CallHandlerAsync(packet, context);
    }

    private Task HandlePlayerPositionAndLookClientPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerPositionAndLookClientPacket();
        packet.Read(ref reader);
        return CallHandlerAsync(packet, context);
    }

    private Task HandlePlayerPositionPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context)
    {
        var packet = new PlayerPositionPacket();
        packet.Read(ref reader);
        return CallHandlerAsync(packet, context);
    }

    private static Task HandleUnknownPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context, int packetId)
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

        var packetSize = (int) reader.Length - 1;
        //TODO Dangerous because we can lose data
        var packetData = packetSize > 0 ? reader.ReadBytesArray(packetSize) : Array.Empty<byte>();
        Console.WriteLine($"Unknown packet: 0x{packetId:X} with data length: {packetData.Length}");
        return Task.CompletedTask;
    }

    private Task CallHandlerAsync<T>(T packet, ClientPacketHandlerContext context) where T : IClientPacket
    {
        return _serviceProvider.GetRequiredService<IClientPacketHandler<T>>().HandleAsync(packet, context);
    }
}
using System.Buffers;
using Microsoft.Extensions.DependencyInjection;
using MineSharp.Core.Packets;
using MineSharp.Extensions;
using MineSharp.Network.Packets;

namespace MineSharp.Network;

public class PacketDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public PacketDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task DispatchPacket(ClientPacketHandlerContext context, ReadOnlySequence<byte> buffer, out SequencePosition finalPosition)
    {
        var reader = new SequenceReader<byte>(buffer);
        var packetId = reader.ReadByte();

        var task = packetId switch
        {
            AnimationPacket.Id => HandlePacket<AnimationPacket>(ref reader, context),
            ChatMessagePacket.Id => HandlePacket<ChatMessagePacket>(ref reader, context),
            EntityActionPacket.Id => HandlePacket<EntityActionPacket>(ref reader, context),
            HandshakeRequestPacket.Id => HandlePacket<HandshakeRequestPacket>(ref reader, context),
            HoldingChangePacket.Id => HandlePacket<HoldingChangePacket>(ref reader, context),
            LoginRequestPacket.Id => HandlePacket<LoginRequestPacket>(ref reader, context),
            PlayerBlockPlacementPacket.Id => HandlePacket<PlayerBlockPlacementPacket>(ref reader, context),
            PlayerDiggingPacket.Id => HandlePacket<PlayerDiggingPacket>(ref reader, context),
            PlayerDisconnectPacket.Id => HandlePacket<PlayerDisconnectPacket>(ref reader, context),
            PlayerLookPacket.Id => HandlePacket<PlayerLookPacket>(ref reader, context),
            PlayerPacket.Id => HandlePacket<PlayerPacket>(ref reader, context),
            PlayerPositionAndLookClientPacket.Id => HandlePacket<PlayerPositionAndLookClientPacket>(ref reader, context),
            PlayerPositionPacket.Id => HandlePacket<PlayerPositionPacket>(ref reader, context),
            RespawnPacket.Id => HandlePacket<RespawnPacket>(ref reader, context),
            UseEntityPacket.Id => HandlePacket<UseEntityPacket>(ref reader, context),
            _ => HandleUnknownPacket(ref reader, context, packetId)
        };

        finalPosition = reader.Position;

        return task;
    }

    private Task HandlePacket<T>(ref SequenceReader<byte> reader, ClientPacketHandlerContext context) where T : IClientPacket, new()
    {
        var packet = new T();
        packet.Read(ref reader);
        return _serviceProvider.GetRequiredService<IClientPacketHandler<T>>().HandleAsync(packet, context);
    }

    private static Task HandleUnknownPacket(ref SequenceReader<byte> reader, ClientPacketHandlerContext context, int packetId)
    {
        //TODO Dangerous because we can lose data
        var packetSize = (int) reader.Length - 1;
        var packetData = packetSize > 0 ? reader.ReadBytesArray(packetSize) : Array.Empty<byte>();
        Console.WriteLine($"[WARN] Unknown packet: 0x{packetId:X} with data length: {packetData.Length}");
        return Task.CompletedTask;
    }
}
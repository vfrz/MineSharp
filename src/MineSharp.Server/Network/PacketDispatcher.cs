using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MineSharp.Extensions;
using MineSharp.Network.Packets;

namespace MineSharp.Network;

//TODO Maybe rewrite this
public class PacketDispatcher
{
    private record struct WrappedPacket(IClientPacket Packet, ClientPacketHandlerContext Context);

    private readonly IServiceProvider _serviceProvider;
    private readonly Channel<WrappedPacket> _packetsChannel;
    private readonly ILogger<PacketDispatcher> _logger;

    public PacketDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetRequiredService<ILogger<PacketDispatcher>>();
        _packetsChannel = Channel.CreateUnbounded<WrappedPacket>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public SequencePosition DispatchPacket(ReadOnlySequence<byte> buffer, ClientPacketHandlerContext context)
    {
        var reader = new SequenceReader<byte>(buffer);
        var packetId = reader.ReadByte();

        var success = packetId switch
        {
            AnimationPacket.Id => EnqueuePacket<AnimationPacket>(ref reader, context),
            ChatMessagePacket.Id => EnqueuePacket<ChatMessagePacket>(ref reader, context),
            CloseWindowPacket.Id => EnqueuePacket<CloseWindowPacket>(ref reader, context),
            EntityActionPacket.Id => EnqueuePacket<EntityActionPacket>(ref reader, context),
            HandshakeRequestPacket.Id => EnqueuePacket<HandshakeRequestPacket>(ref reader, context),
            HoldingChangePacket.Id => EnqueuePacket<HoldingChangePacket>(ref reader, context),
            LoginRequestPacket.Id => EnqueuePacket<LoginRequestPacket>(ref reader, context),
            KeepAlivePacket.Id => EnqueuePacket<KeepAlivePacket>(ref reader, context),
            PlayerBlockPlacementPacket.Id => EnqueuePacket<PlayerBlockPlacementPacket>(ref reader, context),
            PlayerDiggingPacket.Id => EnqueuePacket<PlayerDiggingPacket>(ref reader, context),
            PlayerDisconnectPacket.Id => EnqueuePacket<PlayerDisconnectPacket>(ref reader, context),
            PlayerLookPacket.Id => EnqueuePacket<PlayerLookPacket>(ref reader, context),
            PlayerPacket.Id => EnqueuePacket<PlayerPacket>(ref reader, context),
            PlayerPositionAndLookClientPacket.Id => EnqueuePacket<PlayerPositionAndLookClientPacket>(ref reader, context),
            PlayerPositionPacket.Id => EnqueuePacket<PlayerPositionPacket>(ref reader, context),
            RespawnPacket.Id => EnqueuePacket<RespawnPacket>(ref reader, context),
            TransactionPacket.Id => EnqueuePacket<TransactionPacket>(ref reader, context),
            UpdateSignPacket.Id => EnqueuePacket<UpdateSignPacket>(ref reader, context),
            UseEntityPacket.Id => EnqueuePacket<UseEntityPacket>(ref reader, context),
            WindowClickPacket.Id => EnqueuePacket<WindowClickPacket>(ref reader, context),
            _ => throw new Exception($"Unknown packet with id: {packetId}")
        };

        //TODO Do something if success == false

        return reader.Position;
    }

    private bool EnqueuePacket<T>(ref SequenceReader<byte> reader, ClientPacketHandlerContext context) where T : IClientPacket, new()
    {
        var packet = new T();
        packet.Read(ref reader);
        return _packetsChannel.Writer.TryWrite(new WrappedPacket(packet, context));
    }

    public async Task HandlePacketsAsync()
    {
        while (_packetsChannel.Reader.TryRead(out var wrappedPacket))
        {
            try
            {
                await HandlePacketAsync(wrappedPacket);
            }
            catch (Exception ex)
            {
                //TODO Should we kick the player?
                _logger.LogError(ex, ex.Message);
            }
        }
    }

    private Task HandlePacketAsync(WrappedPacket wrappedPacket)
    {
        return wrappedPacket.Packet.PacketId switch
        {
            AnimationPacket.Id => GetHandler<AnimationPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            ChatMessagePacket.Id => GetHandler<ChatMessagePacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            CloseWindowPacket.Id => GetHandler<CloseWindowPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            EntityActionPacket.Id => GetHandler<EntityActionPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            HandshakeRequestPacket.Id => GetHandler<HandshakeRequestPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            HoldingChangePacket.Id => GetHandler<HoldingChangePacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            LoginRequestPacket.Id => GetHandler<LoginRequestPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            KeepAlivePacket.Id => GetHandler<KeepAlivePacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            PlayerBlockPlacementPacket.Id => GetHandler<PlayerBlockPlacementPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            PlayerDiggingPacket.Id => GetHandler<PlayerDiggingPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            PlayerDisconnectPacket.Id => GetHandler<PlayerDisconnectPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            PlayerLookPacket.Id => GetHandler<PlayerLookPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            PlayerPacket.Id => GetHandler<PlayerPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            PlayerPositionAndLookClientPacket.Id => GetHandler<PlayerPositionAndLookClientPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            PlayerPositionPacket.Id => GetHandler<PlayerPositionPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            RespawnPacket.Id => GetHandler<RespawnPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            TransactionPacket.Id => GetHandler<TransactionPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            UpdateSignPacket.Id => GetHandler<UpdateSignPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            UseEntityPacket.Id => GetHandler<UseEntityPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            WindowClickPacket.Id => GetHandler<WindowClickPacket>().HandleAsync(wrappedPacket.Packet, wrappedPacket.Context),
            _ => throw new Exception()
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IClientPacketHandler<T> GetHandler<T>() where T : IClientPacket => _serviceProvider.GetRequiredService<IClientPacketHandler<T>>();
}
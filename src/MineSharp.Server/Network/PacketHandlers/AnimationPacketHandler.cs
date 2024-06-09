using MineSharp.Network.Packets;

namespace MineSharp.Network.PacketHandlers;

public class AnimationPacketHandler : IClientPacketHandler<AnimationPacket>
{
    public async Task HandleAsync(AnimationPacket packet, ClientPacketHandlerContext context)
    {
        await context.Server.BroadcastPacketAsync(packet, context.RemoteClient);
    }
}
using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class AnimationPacketHandler : IClientPacketHandler<AnimationPacket>
{
    public async Task HandleAsync(AnimationPacket packet, ClientPacketHandlerContext context)
    {
        await context.Server.BroadcastPacketAsync(packet, context.RemoteClient);
    }
}
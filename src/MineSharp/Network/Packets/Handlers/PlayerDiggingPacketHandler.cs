using MineSharp.Core;
using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class PlayerDiggingPacketHandler : IClientPacketHandler<PlayerDiggingPacket>
{
    public async Task HandleAsync(PlayerDiggingPacket packet, ClientPacketHandlerContext context)
    {
        if (packet.Status is PlayerDiggingStatus.Finished)
        {
            await context.Server.World.UpdateBlockAsync(new Vector3i(packet.X, packet.Y, packet.Z), 0);
        }
    }
}
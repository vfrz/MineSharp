using MineSharp.Core;
using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class PlayerDiggingPacketHandler : IClientPacketHandler<PlayerDiggingPacket>
{
    // Sometime client doesn't send a packet with Finished status for some blocks
    private static readonly byte[] InstantlyDestroyedBlocks =
    [
        6,
        31,
        32,
        37,
        38,
        39,
        40,
        50,
        59,
        75,
        76,
        83
    ]; 
    
    public async Task HandleAsync(PlayerDiggingPacket packet, ClientPacketHandlerContext context)
    {
        if (packet.Status is PlayerDiggingStatus.Finished)
        {
            await context.Server.World.UpdateBlockAsync(new Vector3i(packet.X, packet.Y, packet.Z), 0);
        }
        else
        {
            var blockPosition = new Vector3i(packet.X, packet.Y, packet.Z);
            if (packet.Status is PlayerDiggingStatus.Started or PlayerDiggingStatus.Finished
                && InstantlyDestroyedBlocks.Contains(context.Server.World.GetBlockId(blockPosition)))
            {
                await context.Server.World.UpdateBlockAsync(new Vector3i(packet.X, packet.Y, packet.Z), 0);
            }
        }
    }
}
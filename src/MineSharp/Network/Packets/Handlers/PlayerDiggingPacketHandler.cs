using MineSharp.Core;
using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class PlayerDiggingPacketHandler : IClientPacketHandler<PlayerDiggingPacket>
{
    public async Task HandleAsync(PlayerDiggingPacket packet, ClientPacketHandlerContext context)
    {
        if (packet.Status is PlayerDiggingStatus.Finished)
        {
            var blockUpdatePacket = new BlockUpdatePacket
            {
                X = packet.X,
                Y = packet.Y,
                Z = packet.Z,
                BlockId = 0,
                Metadata = 0
            };

            await context.Server.BroadcastPacketAsync(blockUpdatePacket);
            //TODO Update world/chunk
        }
    }
}
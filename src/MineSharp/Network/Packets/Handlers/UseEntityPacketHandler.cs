using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Entities;

namespace MineSharp.Network.Packets.Handlers;

public class UseEntityPacketHandler : IClientPacketHandler<UseEntityPacket>
{
    public async Task HandleAsync(UseEntityPacket packet, ClientPacketHandlerContext context)
    {
        //TODO Make this clean
        if (packet.LeftClick 
            && context.Server.EntityManager.TryGetEntity(packet.TargetEntityId, out var targetEntity)
            && targetEntity is MinecraftPlayer targetPlayer)
        {
            await targetPlayer.SetHealthAsync(targetPlayer.Health - 1);
            await context.Server.BroadcastPacketAsync(new EntityStatusPacket
            {
                EntityId = targetPlayer.EntityId,
                Status = EntityStatus.Hurt
            });
            await context.Server.BroadcastPacketAsync(new EntityVelocity
            {
                EntityId = targetPlayer.EntityId,
                VelocityX = 0,
                VelocityY = 2000,
                VelocityZ = 0
            });
        }
    }
}
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
            && targetEntity is ILivingEntity targetLivingEntity)
        {
            var remotePlayer = context.RemoteClient.Player!;
            
            await context.Server.BroadcastPacketAsync(new EntityStatusPacket
            {
                EntityId = targetLivingEntity.EntityId,
                Status = EntityStatus.Hurt
            });

            var multiplier = targetLivingEntity.KnockBackMultiplier;
            await context.Server.BroadcastPacketAsync(new EntityVelocityPacket
            {
                EntityId = targetLivingEntity.EntityId,
                VelocityX = (short) (-MinecraftMath.SinDegree(remotePlayer.Yaw) * 3000 * multiplier.X),
                VelocityY = (short) (targetLivingEntity.OnGround ? 3000 * multiplier.Y : 0),
                VelocityZ = (short) (MinecraftMath.CosDegree(remotePlayer.Yaw) * 3000 * multiplier.Z)
            });
            //TODO Adapt damage depending on player's weapon/tool
            var damage = 1;
            await targetLivingEntity.SetHealthAsync((short) (targetLivingEntity.Health - damage));
        }
    }
}
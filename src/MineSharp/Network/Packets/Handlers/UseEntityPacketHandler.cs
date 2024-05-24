using MineSharp.Core;
using MineSharp.Entities;

namespace MineSharp.Network.Packets.Handlers;

public class UseEntityPacketHandler : IClientPacketHandler<UseEntityPacket>
{
    public async Task HandleAsync(UseEntityPacket packet, ClientPacketHandlerContext context)
    {
        if (packet.LeftClick
            && context.Server.EntityManager.TryGetEntity(packet.PlayerEntityId, out var playerEntity)
            && playerEntity is Player player
            && context.Server.EntityManager.TryGetEntity(packet.TargetEntityId, out var targetEntity)
            && targetEntity is ILivingEntity targetLivingEntity)
        {
            await player.AttackEntityAsync(targetLivingEntity);
        }
        //TODO Handle other case
    }
}
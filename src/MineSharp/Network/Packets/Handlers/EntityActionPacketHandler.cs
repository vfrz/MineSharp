using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class EntityActionPacketHandler : IClientPacketHandler<EntityActionPacket>
{
    public async Task HandleAsync(EntityActionPacket packet, ClientPacketHandlerContext context)
    {
        if (packet.Action is EntityActionPacket.ActionType.Crouch)
        {
            //TODO Update player entity metadata
        }
        else if (packet.Action is EntityActionPacket.ActionType.Uncrouch)
        {
            //TODO Update player entity metadata
        }
    }
}
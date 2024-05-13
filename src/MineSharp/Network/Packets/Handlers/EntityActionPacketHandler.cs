using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class EntityActionPacketHandler : IClientPacketHandler<EntityActionPacket>
{
    public async Task HandleAsync(EntityActionPacket packet, ClientPacketHandlerContext context)
    {
        switch (packet.Action)
        {
            case EntityActionPacket.ActionType.Crouch:
                await context.RemoteClient.Player!.ToggleCrouchAsync(true);
                break;
            case EntityActionPacket.ActionType.Uncrouch:
                await context.RemoteClient.Player!.ToggleCrouchAsync(false);
                break;
            case EntityActionPacket.ActionType.LeaveBed:
                //TODO Handle if needed
                break;
            default:
                throw new Exception();
        }
    }
}
using MineSharp.Core.Packets;

namespace MineSharp.Packets.Handlers;

public class PlayerLookPacketHandler : IClientPacketHandler<PlayerLookPacket>
{
    public Task HandleAsync(PlayerLookPacket packet, ClientPacketHandlerContext context)
    {
        var player = context.RemoteClient.Player!;
        player.Yaw = packet.Yaw;
        player.Pitch = packet.Pitch;
        player.OnGround = packet.OnGround;

        player.PositionDirty = true;

        return Task.CompletedTask;
    }
}
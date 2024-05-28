using MineSharp.Network.Packets;

namespace MineSharp.Network.PacketHandlers;

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
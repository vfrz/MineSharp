using MineSharp.Core;
using MineSharp.Network.Packets;

namespace MineSharp.Network.PacketHandlers;

public class PlayerPositionAndLookClientPacketHandler : IClientPacketHandler<PlayerPositionAndLookClientPacket>
{
    public Task HandleAsync(PlayerPositionAndLookClientPacket packet, ClientPacketHandlerContext context)
    {
        var player = context.RemoteClient.Player!;
        player.Position = new Vector3d(packet.X, packet.Y, packet.Z);
        player.Stance = packet.Stance;
        player.OnGround = packet.OnGround;

        player.Yaw = packet.Yaw;
        player.Pitch = packet.Pitch;

        player.PositionDirty = true; //TODO Calculate this

        return Task.CompletedTask;
    }
}
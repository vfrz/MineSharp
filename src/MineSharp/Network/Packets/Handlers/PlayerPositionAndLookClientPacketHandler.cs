using MineSharp.Core;
using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class PlayerPositionAndLookClientPacketHandler : IClientPacketHandler<PlayerPositionAndLookClientPacket>
{
    public Task HandleAsync(PlayerPositionAndLookClientPacket packet, ClientPacketHandlerContext context)
    {
        var player = context.RemoteClient.Player!;
        player.Position = new Vector3(packet.X, packet.Y, packet.Z);
        player.Yaw = packet.Yaw;
        player.Pitch = packet.Pitch;
        player.Stance = packet.Stance;
        player.OnGround = packet.OnGround;

        player.PositionDirty = true; //TODO Calculate this

        return Task.CompletedTask;
    }
}
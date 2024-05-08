using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class PlayerPositionAndLookClientPacketHandler : IClientPacketHandler<PlayerPositionAndLookClientPacket>
{
    public Task HandleAsync(PlayerPositionAndLookClientPacket packet, ClientPacketHandlerContext context)
    {
        var player = context.RemoteClient.Player!;
        player.X = packet.X;
        player.Z = packet.Z;
        player.Y = packet.Y;
        player.Yaw = packet.Yaw;
        player.Pitch = packet.Pitch;
        player.Stance = packet.Stance;
        player.OnGround = packet.OnGround;

        player.PositionDirty = true; //TODO Calculate this

        return Task.CompletedTask;
    }
}
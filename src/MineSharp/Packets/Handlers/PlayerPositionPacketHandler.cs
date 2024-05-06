using MineSharp.Core.Packets;

namespace MineSharp.Packets.Handlers;

public class PlayerPositionPacketHandler : IClientPacketHandler<PlayerPositionPacket>
{
    public Task HandleAsync(PlayerPositionPacket packet, ClientPacketHandlerContext context)
    {
        var player = context.RemoteClient.Player!;
        player.X = packet.X;
        player.Z = packet.Z;
        player.Y = packet.Y;
        player.Stance = packet.Stance;
        player.OnGround = packet.OnGround;

        player.PositionDirty = true; //TODO Calculate this

        return Task.CompletedTask;
    }
}
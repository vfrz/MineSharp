using MineSharp.Core;
using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class PlayerPositionPacketHandler : IClientPacketHandler<PlayerPositionPacket>
{
    public Task HandleAsync(PlayerPositionPacket packet, ClientPacketHandlerContext context)
    {
        var player = context.RemoteClient.Player!;
        player.Position = new Vector3(packet.X, packet.Y, packet.Z);
        player.Stance = packet.Stance;
        player.OnGround = packet.OnGround;

        player.PositionDirty = true; //TODO Calculate this

        return Task.CompletedTask;
    }
}
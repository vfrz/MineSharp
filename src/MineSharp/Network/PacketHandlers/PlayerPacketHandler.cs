using MineSharp.Network.Packets;

namespace MineSharp.Network.PacketHandlers;

public class PlayerPacketHandler : IClientPacketHandler<PlayerPacket>
{
    public Task HandleAsync(PlayerPacket packet, ClientPacketHandlerContext context)
    {
        var player = context.RemoteClient.Player!;
        player.OnGround = packet.OnGround;
        return Task.CompletedTask;
    }
}
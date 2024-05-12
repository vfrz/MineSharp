using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class PlayerPacketHandler : IClientPacketHandler<PlayerPacket>
{
    public Task HandleAsync(PlayerPacket packet, ClientPacketHandlerContext context)
    {
        var player = context.RemoteClient.Player!;
        player.OnGround = packet.OnGround;
        return Task.CompletedTask;
    }
}
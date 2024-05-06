using MineSharp.Core.Packets;

namespace MineSharp.Packets.Handlers;

public class PlayerPacketHandler : IClientPacketHandler<PlayerPacket>
{
    public Task HandleAsync(PlayerPacket packet, ClientPacketHandlerContext context)
    {
        var player = context.RemoteClient.Player!;
        player.OnGround = packet.OnGround;

        //player.PositionDirty = true; //TODO Not sure
        
        return Task.CompletedTask;
    }
}
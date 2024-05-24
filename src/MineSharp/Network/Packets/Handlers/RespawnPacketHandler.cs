namespace MineSharp.Network.Packets.Handlers;

public class RespawnPacketHandler : IClientPacketHandler<RespawnPacket>
{
    public async Task HandleAsync(RespawnPacket packet, ClientPacketHandlerContext context)
    {
        var player = context.RemoteClient.Player!;
        await player.RespawnAsync(packet.Dimension);
    }
}
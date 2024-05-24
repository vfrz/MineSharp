namespace MineSharp.Network.Packets.Handlers;

public class PlayerDisconnectPacketHandler : IClientPacketHandler<PlayerDisconnectPacket>
{
    public async Task HandleAsync(PlayerDisconnectPacket packet, ClientPacketHandlerContext context)
    {
        await context.RemoteClient.DisconnectSocketAsync();
    }
}
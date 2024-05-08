using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class PlayerDisconnectPacketHandler : IClientPacketHandler<PlayerDisconnectPacket>
{
    public async Task HandleAsync(PlayerDisconnectPacket packet, ClientPacketHandlerContext context)
    {
        //TODO this is ugly but for testing
        await context.RemoteClient.DisconnectSocketAsync();
    }
}
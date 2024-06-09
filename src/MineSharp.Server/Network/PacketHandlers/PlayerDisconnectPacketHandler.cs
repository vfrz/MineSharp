using MineSharp.Network.Packets;

namespace MineSharp.Network.PacketHandlers;

public class PlayerDisconnectPacketHandler : IClientPacketHandler<PlayerDisconnectPacket>
{
    public async Task HandleAsync(PlayerDisconnectPacket packet, ClientPacketHandlerContext context)
    {
        await context.RemoteClient.DisconnectSocketAsync();
    }
}
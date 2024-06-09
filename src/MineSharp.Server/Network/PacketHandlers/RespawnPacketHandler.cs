using MineSharp.Network.Packets;

namespace MineSharp.Network.PacketHandlers;

public class RespawnPacketHandler : IClientPacketHandler<RespawnPacket>
{
    public async Task HandleAsync(RespawnPacket packet, ClientPacketHandlerContext context)
    {
        var player = context.RemoteClient.Player!;
        await player.RespawnAsync(packet.Dimension);
    }
}
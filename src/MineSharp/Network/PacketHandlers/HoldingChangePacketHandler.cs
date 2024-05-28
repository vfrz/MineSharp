using MineSharp.Network.Packets;

namespace MineSharp.Network.PacketHandlers;

public class HoldingChangePacketHandler : IClientPacketHandler<HoldingChangePacket>
{
    public async Task HandleAsync(HoldingChangePacket packet, ClientPacketHandlerContext context)
    {
        var player = context.RemoteClient.Player!;
        await player.HoldItemChangedAsync(packet.SlotId);
    }
}
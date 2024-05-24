namespace MineSharp.Network.Packets.Handlers;

public class HoldingChangePacketHandler : IClientPacketHandler<HoldingChangePacket>
{
    public async Task HandleAsync(HoldingChangePacket packet, ClientPacketHandlerContext context)
    {
        var player = context.RemoteClient.Player!;
        await player.HoldItemChangedAsync(packet.SlotId);
    }
}
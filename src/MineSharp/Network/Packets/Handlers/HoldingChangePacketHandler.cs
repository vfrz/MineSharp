using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class HoldingChangePacketHandler : IClientPacketHandler<HoldingChangePacket>
{
    public Task HandleAsync(HoldingChangePacket packet, ClientPacketHandlerContext context)
    {
        return Task.CompletedTask;
    }
}
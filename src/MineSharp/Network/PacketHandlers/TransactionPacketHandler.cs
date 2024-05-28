using MineSharp.Network.Packets;

namespace MineSharp.Network.PacketHandlers;

public class TransactionPacketHandler : IClientPacketHandler<TransactionPacket>
{
    public Task HandleAsync(TransactionPacket packet, ClientPacketHandlerContext context)
    {
        return Task.CompletedTask;
    }
}
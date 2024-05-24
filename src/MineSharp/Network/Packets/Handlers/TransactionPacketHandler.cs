namespace MineSharp.Network.Packets.Handlers;

public class TransactionPacketHandler : IClientPacketHandler<TransactionPacket>
{
    public Task HandleAsync(TransactionPacket packet, ClientPacketHandlerContext context)
    {
        return Task.CompletedTask;
    }
}
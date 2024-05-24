namespace MineSharp.Network.Packets.Handlers;

public class CloseWindowPacketHandler : IClientPacketHandler<CloseWindowPacket>
{
    public Task HandleAsync(CloseWindowPacket packet, ClientPacketHandlerContext context)
    {
        return Task.CompletedTask;
    }
}
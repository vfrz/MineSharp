namespace MineSharp.Network.Packets.Handlers;

public class KeepAlivePacketHandler : IClientPacketHandler<KeepAlivePacket>
{
    public Task HandleAsync(KeepAlivePacket packet, ClientPacketHandlerContext context)
    {
        return Task.CompletedTask;
    }
}
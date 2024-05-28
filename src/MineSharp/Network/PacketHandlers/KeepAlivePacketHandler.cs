using MineSharp.Network.Packets;

namespace MineSharp.Network.PacketHandlers;

public class KeepAlivePacketHandler : IClientPacketHandler<KeepAlivePacket>
{
    public Task HandleAsync(KeepAlivePacket packet, ClientPacketHandlerContext context)
    {
        //TODO Implement, disconnect player if no keep alive in a certain time
        return Task.CompletedTask;
    }
}
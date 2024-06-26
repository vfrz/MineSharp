using MineSharp.Network.Packets;

namespace MineSharp.Network.PacketHandlers;

public class CloseWindowPacketHandler : IClientPacketHandler<CloseWindowPacket>
{
    public Task HandleAsync(CloseWindowPacket packet, ClientPacketHandlerContext context)
    {
        return Task.CompletedTask;
    }
}
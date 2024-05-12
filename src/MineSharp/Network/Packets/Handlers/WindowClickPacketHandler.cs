using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class WindowClickPacketHandler : IClientPacketHandler<WindowClickPacket>
{
    public Task HandleAsync(WindowClickPacket packet, ClientPacketHandlerContext context)
    {
        return Task.CompletedTask;
    }
}
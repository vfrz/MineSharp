using Microsoft.Extensions.Logging;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets.Handlers;

public class WindowClickPacketHandler : IClientPacketHandler<WindowClickPacket>
{
    public Task HandleAsync(WindowClickPacket packet, ClientPacketHandlerContext context)
    {
        context.Server.GetLogger<WindowClickPacketHandler>().LogDebug(packet.ToDebugString());
        return Task.CompletedTask;
    }
}
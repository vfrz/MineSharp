using Microsoft.Extensions.Logging;
using MineSharp.Extensions;
using MineSharp.Network.Packets;

namespace MineSharp.Network.PacketHandlers;

public class WindowClickPacketHandler : IClientPacketHandler<WindowClickPacket>
{
    public Task HandleAsync(WindowClickPacket packet, ClientPacketHandlerContext context)
    {
        context.Server.GetLogger<WindowClickPacketHandler>().LogDebug(packet.ToDebugString());
        return Task.CompletedTask;
    }
}
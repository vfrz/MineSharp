using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class HandshakeRequestPacketHandler : IClientPacketHandler<HandshakeRequestPacket>
{
    public async Task HandleAsync(HandshakeRequestPacket packet, ClientPacketHandlerContext context)
    {
        var response = new HandshakeResponsePacket
        {
            ConnectionHash = "-"
        };
        await context.RemoteClient.SendPacketAsync(response);
    }
}
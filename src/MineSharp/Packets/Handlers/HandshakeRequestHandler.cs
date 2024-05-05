using MineSharp.Core.Packets;

namespace MineSharp.Packets.Handlers;

public class HandshakeRequestHandler : IPacketHandler<HandshakeRequest>
{
    public async ValueTask HandleAsync(HandshakeRequest command, CancellationToken cancellationToken)
    {
        command.Client.State = command.NextState;
        
        using var session = command.Client.SocketWrapper.StartWriting();
        session.WriteByte(0x2);
        await session.WriteStringAsync("-");
    }
}
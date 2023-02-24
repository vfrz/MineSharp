using MineSharp.Core.Packets;

namespace MineSharp.Packets.Handlers;

public class PingRequestHandler : IPacketHandler<PingRequest>
{
    public async ValueTask HandleAsync(PingRequest command, CancellationToken cancellationToken)
    {
        var socket = command.Client.SocketWrapper;

        using (var session = socket.StartWriting())
        {
            await session.WriteVarIntAsync(9);
            await session.WriteVarIntAsync(1);
            await session.WriteLongAsync(command.Payload);
        }
        
        await command.Client.DisconnectAsync();
    }
}
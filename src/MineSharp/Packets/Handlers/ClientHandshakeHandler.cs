using MineSharp.Core.Packets;

namespace MineSharp.Packets.Handlers;

public class ClientHandshakeHandler : IPacketHandler<ClientHandshake>
{
    public ValueTask HandleAsync(ClientHandshake command, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}
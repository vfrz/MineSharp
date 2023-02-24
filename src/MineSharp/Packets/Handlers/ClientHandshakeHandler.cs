using MineSharp.Core.Packets;

namespace MineSharp.Packets.Handlers;

public class ClientHandshakeHandler : IPacketHandler<ClientHandshake>
{
    public ValueTask HandleAsync(ClientHandshake command, CancellationToken cancellationToken)
    {
        command.Client.State = command.NextState;
        command.Client.ProtocolVersion = command.ProtocolVersion;
        return ValueTask.CompletedTask;
    }
}
using MineSharp.Core.Packets;

namespace MineSharp.Packets.Handlers;

public class UnknownPacketHandler : IPacketHandler<UnknownPacket>
{
    public ValueTask HandleAsync(UnknownPacket command, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}
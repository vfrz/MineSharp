using Mediator;
using MineSharp.Commands;

namespace MineSharp.Handlers;

public class UnknownPacketHandler : ICommandHandler<UnknownPacket>
{
    public ValueTask<Unit> Handle(UnknownPacket command, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(Unit.Value);
    }
}
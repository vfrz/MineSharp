using Mediator;
using MineSharp.Commands;

namespace MineSharp.Handlers;

public class ClientHandshakeHandler : ICommandHandler<ClientHandshake>
{
    public ValueTask<Unit> Handle(ClientHandshake command, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(Unit.Value);
    }
}
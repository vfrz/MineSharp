using Mediator;

namespace MineSharp.Core.Packets;

public interface IPacketHandler<in T> : ICommandHandler<T> where T : ICommand
{
    ValueTask HandleAsync(T command, CancellationToken cancellationToken);
    
    async ValueTask<Unit> ICommandHandler<T, Unit>.Handle(T command, CancellationToken cancellationToken)
    {
        await HandleAsync(command, cancellationToken);
        return Unit.Value;
    }
}
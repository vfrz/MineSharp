using Mediator;
using MineSharp.Commands;

namespace MineSharp.Handlers;

public class PingRequestHandler : ICommandHandler<PingRequest>
{
    public async ValueTask<Unit> Handle(PingRequest command, CancellationToken cancellationToken)
    {
        var socket = command.Client.SocketWrapper;

        using (var session = socket.StartWriting())
        {
            await session.WriteVarIntAsync(9);
            await session.WriteVarIntAsync(1);
            await session.WriteLongAsync(command.Payload);
        }
        
        await command.Client.DisconnectAsync();
        
        return Unit.Value;
    }
}
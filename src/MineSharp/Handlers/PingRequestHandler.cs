using Mediator;
using MineSharp.Commands;

namespace MineSharp.Handlers;

public class PingRequestHandler : ICommandHandler<PingRequest>
{
    public async ValueTask<Unit> Handle(PingRequest command, CancellationToken cancellationToken)
    {
        var socket = command.Client.SocketWrapper;

        await socket.WriteVarIntAsync(9);
        await socket.WriteVarIntAsync(1);
        await socket.WriteLongAsync(command.Payload);

        await command.Client.DisconnectAsync();
        
        return Unit.Value;
    }
}
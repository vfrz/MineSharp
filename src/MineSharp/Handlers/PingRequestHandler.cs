using Mediator;
using MineSharp.Commands;

namespace MineSharp.Handlers;

public class PingRequestHandler : ICommandHandler<PingRequest>
{
    public async ValueTask<Unit> Handle(PingRequest command, CancellationToken cancellationToken)
    {
        var socket = command.Client.SocketWrapper;

        await socket.WriteVarInt(9);
        await socket.WriteVarInt(1);
        socket.WriteLong(command.Payload);

        await command.Client.DisconnectAsync();
        
        return Unit.Value;
    }
}
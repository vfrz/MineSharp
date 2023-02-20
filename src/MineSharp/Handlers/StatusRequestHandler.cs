using System.Text.Json;
using Mediator;
using Microsoft.Extensions.Options;
using MineSharp.Commands;
using MineSharp.Core;
using MineSharp.Extensions;
using MineSharp.Network;
using MineSharp.ServerStatus;

namespace MineSharp.Handlers;

public class StatusRequestHandler : ICommandHandler<StatusRequest>
{
    private readonly MinecraftServer _server;
    private readonly IOptions<ServerConfiguration> _configuration;
    
    public StatusRequestHandler(MinecraftServer server, IOptions<ServerConfiguration> configuration)
    {
        _server = server;
        _configuration = configuration;
    }

    public async ValueTask<Unit> Handle(StatusRequest command, CancellationToken cancellationToken)
    {
        var status = new ServerStatusResponse
        {
            Description = new ServerStatusDescription
            {
                Text = _configuration.Value.Description
            },
            Players = new ServerStatusPlayers
            {
                Max = _configuration.Value.MaxPlayers,
                Online = _server.Clients.Count(c => c.Player is not null)
            },
            Version = new ServerStatusVersion
            {
                Name = ServerConstants.VersionName,
                Protocol = ServerConstants.ProtocolVersion
            }
        };
 
        var statusBytes = JsonSerializer.Serialize(status).ToVarString();

        var socket = command.Client.SocketWrapper;
        
        await socket.WriteVarInt(statusBytes.Length + 1);
        await socket.WriteVarInt(0);
        socket.WriteBytes(statusBytes);
        
        return Unit.Value;
    }
}
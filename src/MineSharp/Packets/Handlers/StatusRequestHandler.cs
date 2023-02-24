using System.Text.Json;
using Microsoft.Extensions.Options;
using MineSharp.Configuration;
using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Extensions;
using MineSharp.Network;
using MineSharp.ServerStatus;

namespace MineSharp.Packets.Handlers;

public class StatusRequestHandler : IPacketHandler<StatusRequest>
{
    private readonly MinecraftServer _server;
    private readonly IOptions<ServerConfiguration> _configuration;

    public StatusRequestHandler(MinecraftServer server, IOptions<ServerConfiguration> configuration)
    {
        _server = server;
        _configuration = configuration;
    }

    public async ValueTask HandleAsync(StatusRequest command, CancellationToken cancellationToken)
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
            },
            Favicon = _server.FaviconBase64
        };

        var statusBytes = JsonSerializer.Serialize(status).ToVarString();

        var socket = command.Client.SocketWrapper;

        using var session = socket.StartWriting();
        await session.WriteVarIntAsync(statusBytes.Length + 1);
        await session.WriteVarIntAsync(0);
        await session.WriteBytesAsync(statusBytes);
    }
}
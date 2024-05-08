using Microsoft.Extensions.Logging;
using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class PlayerPacketHandler : IClientPacketHandler<PlayerPacket>
{
    private readonly ILogger<PlayerPacketHandler> _logger;

    public PlayerPacketHandler(ILogger<PlayerPacketHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(PlayerPacket packet, ClientPacketHandlerContext context)
    {
        //_logger.LogInformation($"PLAYER: {context.RemoteClient.NetworkId}");
        
        var player = context.RemoteClient.Player!;
        player.OnGround = packet.OnGround;

        //player.PositionDirty = true; //TODO Not sure
        
        return Task.CompletedTask;
    }
}
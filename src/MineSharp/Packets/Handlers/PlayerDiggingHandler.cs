using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Network;

namespace MineSharp.Packets.Handlers;

public class PlayerDiggingHandler : IPacketHandler<PlayerDigging>
{
    private readonly MinecraftServer _server;

    public PlayerDiggingHandler(MinecraftServer server)
    {
        _server = server;
    }

    public async ValueTask HandleAsync(PlayerDigging command, CancellationToken cancellationToken)
    {
        if (command.Status is PlayerDiggingStatus.Finished)
        {
            var coordinates = new Coordinates3D(command.X, command.Z, command.Y);

            await Parallel.ForEachAsync(_server.Clients, async (client, token) =>
            {
                using var session = client.SocketWrapper.StartWriting();
                session.WriteByte(0x35);
                await session.WriteIntAsync(coordinates.X);
                session.WriteSByte(coordinates.Y);
                await session.WriteIntAsync(coordinates.Z);
                session.WriteByte(0);
                session.WriteByte(0);
            });

            //TODO Update world/chunk
        }
    }
}
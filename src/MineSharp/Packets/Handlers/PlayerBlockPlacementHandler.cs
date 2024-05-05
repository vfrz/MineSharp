using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Network;

namespace MineSharp.Packets.Handlers;

public class PlayerBlockPlacementHandler : IPacketHandler<PlayerBlockPlacement>
{
    private readonly MinecraftServer _server;

    public PlayerBlockPlacementHandler(MinecraftServer server)
    {
        _server = server;
    }

    public async ValueTask HandleAsync(PlayerBlockPlacement command, CancellationToken cancellationToken)
    {
        if (command.BlockId == -1 || command.Direction == -1)
            return;

        var coordinates = new Coordinates3D(command.X, command.Z, command.Y);
        ApplyDirectionToCoordinates(ref coordinates, command.Direction);
        
        await Parallel.ForEachAsync(_server.Clients, async (client, token) =>
        {
            using var session = client.SocketWrapper.StartWriting();
            session.WriteByte(0x35);
            await session.WriteIntAsync(coordinates.X);
            session.WriteSByte(coordinates.Y);
            await session.WriteIntAsync(coordinates.Z);
            session.WriteByte((byte) command.BlockId);
            session.WriteByte(0);
        });
        
        //TODO Update world/chunk
    }

    private static void ApplyDirectionToCoordinates(ref Coordinates3D coordinates, sbyte direction)
    {
        if (direction == 0)
            coordinates.Y--;
        else if (direction == 1)
            coordinates.Y++;
        else if (direction == 2)
            coordinates.Z--;
        else if (direction == 3)
            coordinates.Z++;
        else if (direction == 4)
            coordinates.X--;
        else if (direction == 5)
            coordinates.X++;
    }
}
using MineSharp.Core;
using MineSharp.Core.Packets;

namespace MineSharp.Packets.Handlers;

public class PlayerBlockPlacementPacketHandler : IClientPacketHandler<PlayerBlockPlacementPacket>
{
    public async Task HandleAsync(PlayerBlockPlacementPacket packet, ClientPacketHandlerContext context)
    {
        if (packet.BlockId == -1 || packet.Direction == -1)
            return;

        var coordinates = new Coordinates3D(packet.X, packet.Z, packet.Y);
        ApplyDirectionToCoordinates(ref coordinates, packet.Direction);

        await Parallel.ForEachAsync(context.Server.Clients, async (client, token) =>
        {
            using var session = client.SocketWrapper.StartWriting();
            session.WriteByte(0x35);
            await session.WriteIntAsync(coordinates.X);
            session.WriteSByte(coordinates.Y);
            await session.WriteIntAsync(coordinates.Z);
            session.WriteByte((byte) packet.BlockId);
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
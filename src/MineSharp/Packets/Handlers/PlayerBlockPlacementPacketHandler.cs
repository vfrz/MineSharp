using MineSharp.Core;
using MineSharp.Core.Packets;

namespace MineSharp.Packets.Handlers;

public class PlayerBlockPlacementPacketHandler : IClientPacketHandler<PlayerBlockPlacementPacket>
{
    public async Task HandleAsync(PlayerBlockPlacementPacket packet, ClientPacketHandlerContext context)
    {
        if (packet.Direction == -1)
            return;

        if (packet.BlockId == -1)
        {
            await context.Server.BroadcastPacketAsync(new ThunderboltPacket
            {
                X = packet.X * 32,
                Y = packet.Y * 32,
                Z = packet.Z * 32,
                EntityId = 42
            });
            return;
        }

        var coordinates = new Coordinates3D(packet.X, packet.Z, packet.Y);
        ApplyDirectionToCoordinates(ref coordinates, packet.Direction);

        await context.Server.BroadcastPacketAsync(new BlockUpdatePacket
        {
            X = coordinates.X,
            Y = coordinates.Y,
            Z = coordinates.Z,
            BlockId = (byte) packet.BlockId,
            Metadata = 0
        });

        //TODO Update world/chunk
    }

    //TODO Move this method somewhere else
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
using MineSharp.Core;
using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class PlayerBlockPlacementPacketHandler : IClientPacketHandler<PlayerBlockPlacementPacket>
{
    public async Task HandleAsync(PlayerBlockPlacementPacket packet, ClientPacketHandlerContext context)
    {
        if (packet.Direction == -1)
            return;

        var coordinates = new Vector3i(packet.X, packet.Y, packet.Z);
        var directedCoordinates = ApplyDirectionToCoordinates(coordinates, packet.Direction);

        if (packet.BlockId == -1)
        {
            /*await context.Server.BroadcastPacketAsync(new BlockUpdatePacket
            {
                X = directedCoordinates.X,
                Y = (sbyte) directedCoordinates.Y,
                Z = directedCoordinates.Z,
                BlockId = 31,
                Metadata = 0x1
            });*/

            await context.RemoteClient.SendMessageAsync($"Position: {packet.X} {packet.Y} {packet.Z}");
            //await context.Server.SpawnMobAsync(new Sheep(Sheep.ColorType.Red), directedCoordinates);

            /*await context.Server.BroadcastPacketAsync(new ThunderboltPacket
            {
                X = packet.X * 32,
                Y = packet.Y * 32,
                Z = packet.Z * 32,
                EntityId = 42
            });*/
        }
        else
        {
            await context.Server.World.UpdateBlockAsync(directedCoordinates, (byte) packet.BlockId);
        }
    }

    //TODO Move this method somewhere else
    private static Vector3i ApplyDirectionToCoordinates(Vector3i coordinates, sbyte direction)
    {
        if (direction == 0)
            return coordinates + new Vector3i(0, -1, 0);
        if (direction == 1)
            return coordinates + new Vector3i(0, 1, 0);
        if (direction == 2)
            return coordinates + new Vector3i(0, 0, -1);
        if (direction == 3)
            return coordinates + new Vector3i(0, 0, 1);
        if (direction == 4)
            return coordinates + new Vector3i(-1, 0, 0);
        if (direction == 5)
            return coordinates + new Vector3i(1, 0, 0);
        throw new Exception($"Unknown direction: {direction}");
    }
}
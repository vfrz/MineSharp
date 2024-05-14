using MineSharp.Core;
using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class PlayerBlockPlacementPacketHandler : IClientPacketHandler<PlayerBlockPlacementPacket>
{
    public async Task HandleAsync(PlayerBlockPlacementPacket packet, ClientPacketHandlerContext context)
    {
        //TODO Check distance from player
        if (packet.Direction == -1)
            return;

        if (packet.ItemId == -1)
        {
            await context.RemoteClient.SendChatAsync($"Position: {packet.X} {packet.Y} {packet.Z}");
        }
        else if (packet.ItemId == 50)
        {
            await PlaceTorchAsync(packet, context);
        }
        else if (packet.ItemId == 323)
        {
            await PlaceSignAsync(packet, context);
        }
        else if (packet.ItemId == 324)
        {
            await PlaceDoorAsync(packet, context);
        }
        else
        {
            var coordinates = new Vector3i(packet.X, packet.Y, packet.Z);
            var directedCoordinates = ApplyDirectionToPosition(coordinates, packet.Direction);
            var player = context.RemoteClient.Player!;

            var success = await player.SubtractSelectedItemAsync();
            if (success)
            {
                await context.Server.World.SetBlockAsync(directedCoordinates, (byte) packet.ItemId);
            }
            else
            {
                await context.Server.World.SetBlockAsync(directedCoordinates, 0);
            }
        }
    }

    private async Task PlaceTorchAsync(PlayerBlockPlacementPacket packet, ClientPacketHandlerContext context)
    {
        var position = new Vector3i(packet.X, packet.Y, packet.Z);
        var directedPosition = ApplyDirectionToPosition(position, packet.Direction);

        // Torches can't be placed at the bottom of a block
        if (packet.Direction == 0)
        {
            //TODO Give back the item to the player
            await CancelPlacementAsync(directedPosition, context);
            return;
        }

        var orientationMetadata = (byte) packet.Direction;
        await context.Server.World.SetBlockAsync(directedPosition, (byte) packet.ItemId, orientationMetadata);
    }

    private async Task PlaceSignAsync(PlayerBlockPlacementPacket packet, ClientPacketHandlerContext context)
    {
        var position = new Vector3i(packet.X, packet.Y, packet.Z);
        var directedPosition = ApplyDirectionToPosition(position, packet.Direction);

        // Signs can't be placed at the bottom of a block
        if (packet.Direction == 0)
        {
            //TODO Give back the item to the player
            await CancelPlacementAsync(directedPosition, context);
            return;
        }

        var onFloor = packet.Direction == 1;
        var blockId = (byte) (onFloor ? 63 : 68);
        var orientationMetadata = (byte) packet.Direction;
        if (onFloor)
        {
            var rotation = context.RemoteClient.Player!.Yaw + 180 % 360;
            if (rotation < 0)
                rotation += 360;
            orientationMetadata = (byte) (rotation / 22.5);
        }

        await context.Server.World.SetBlockAsync(directedPosition, blockId, orientationMetadata);
    }

    private async Task PlaceDoorAsync(PlayerBlockPlacementPacket packet, ClientPacketHandlerContext context)
    {
        var onFloor = packet.Direction == 1;
        if (!onFloor)
            return;
        //TODO
        await CancelPlacementAsync(new Vector3i(packet.X, packet.Y + 1, packet.Z), context);
        await CancelPlacementAsync(new Vector3i(packet.X, packet.Y + 2, packet.Z), context);
        await context.RemoteClient.SendChatAsync("Doors are not supported yet :\\");
    }

    private async Task CancelPlacementAsync(Vector3i position, ClientPacketHandlerContext context)
    {
        await context.RemoteClient.SendPacketAsync(new BlockUpdatePacket
        {
            X = position.X,
            Y = (sbyte) position.Y,
            Z = position.Z,
            BlockId = 0,
            Metadata = 0
        });
    }

    //TODO Move this method somewhere else
    private static Vector3i ApplyDirectionToPosition(Vector3i coordinates, sbyte direction)
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
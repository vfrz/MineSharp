using MineSharp.Blocks;
using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Items;

namespace MineSharp.Network.Packets.Handlers;

public class PlayerBlockPlacementPacketHandler : IClientPacketHandler<PlayerBlockPlacementPacket>
{
    public async Task HandleAsync(PlayerBlockPlacementPacket packet, ClientPacketHandlerContext context)
    {
        //TODO Check distance from player
        if (packet.Direction == -1)
            return;

        var itemId = packet.ItemId;

        if (itemId is ItemId.Empty)
        {
            if (context.Server.Configuration.Debug)
            {
                await context.RemoteClient.SendChatAsync($"Position: {packet.X} {packet.Y} {packet.Z}");
            }

            return;
        }

        var player = context.RemoteClient.Player!;
        var holdItemStack = player.HoldItemStack;

        if (holdItemStack.Count == packet.Amount)
            return;

        if (holdItemStack.ItemId != packet.ItemId)
        {
            await context.RemoteClient.KickAsync("Invalid block placement");
            return;
        }

        var coordinates = new Vector3i(packet.X, packet.Y, packet.Z);
        var directedCoordinates = ApplyDirectionToPosition(coordinates, packet.Direction);

        var popItemSuccess = await player.PopSelectedItemStackAsync();

        if (!popItemSuccess)
        {
            var actualBlock = await context.Server.World.GetBlockAsync(directedCoordinates);
            await context.RemoteClient.SendPacketAsync(new BlockUpdatePacket
            {
                X = directedCoordinates.X,
                Y = (sbyte)directedCoordinates.Y,
                Z = directedCoordinates.Z,
                BlockId = actualBlock.BlockId,
                Metadata = actualBlock.Metadata
            });
            return;
        }

        if (packet.ItemId is ItemId.TorchBlock)
        {
            await PlaceTorchAsync(packet, context);
        }
        else if (packet.ItemId is ItemId.Sign)
        {
            await PlaceSignAsync(packet, context);
        }
        else if (packet.ItemId is ItemId.Door)
        {
            await PlaceDoorAsync(packet, context);
        }
        else
        {
            if (popItemSuccess)
            {
                await context.Server.World.SetBlockAsync(directedCoordinates, (BlockId)packet.ItemId, (byte)packet.Metadata!);
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
        // TODO Actually they can if there is no block at Y-1 and a block at Y-1 and (X-1 or X+1 or Z-1 or Z+1)
        if (packet.Direction == 0)
        {
            //TODO Give back the item to the player
            await CancelPlacementAsync(directedPosition, context);
            return;
        }

        var orientationMetadata = (byte)packet.Direction;
        await context.Server.World.SetBlockAsync(directedPosition, BlockId.Torch, orientationMetadata);
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
        var orientationMetadata = (byte)packet.Direction;
        if (onFloor)
        {
            var rotation = context.RemoteClient.Player!.Yaw + 180 % 360;
            if (rotation < 0)
                rotation += 360;
            orientationMetadata = (byte)(rotation / 22.5);
        }

        var blockId = onFloor ? BlockId.FloorSign : BlockId.WallSign;

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
            Y = (sbyte)position.Y,
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
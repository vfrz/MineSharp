using MineSharp.Content;
using MineSharp.Content.Items;
using MineSharp.Network.Packets;
using MineSharp.Numerics;
using MineSharp.TileEntities;
using MineSharp.World;

namespace MineSharp.Network.PacketHandlers;

public class PlayerBlockPlacementPacketHandler : IClientPacketHandler<PlayerBlockPlacementPacket>
{
    public async Task HandleAsync(PlayerBlockPlacementPacket packet, ClientPacketHandlerContext context)
    {
        if (packet.ItemId.IsItem())
        {
            var itemInfo = ItemInfoProvider.Get(packet.ItemId);

            if (packet.ItemId is ItemId.Sign)
            {
                await PlaceSignAsync(packet, context);
            }
            else if (packet.ItemId is ItemId.WoodenDoor)
            {
                await PlaceDoorAsync(packet, context);
            }
            else if (itemInfo is FoodItemInfo foodItemInfo)
            {
                if (packet.Direction != -1)
                    return;

                await context.RemoteClient.Player!.AddHealthAsync(foodItemInfo.HealthRestore);
            }
        }
        else if (packet.ItemId.IsBlock())
        {
            if (packet.Direction == -1)
                return;

            //TODO Check distance from player
            var player = context.RemoteClient.Player!;
            var holdItemStack = player.HoldItemStack;

            if (holdItemStack.Count == packet.Amount)
                return;

            if (holdItemStack.ItemId != packet.ItemId)
            {
                await context.RemoteClient.KickAsync("Invalid block placement");
                return;
            }

            var position = packet.PositionAsVector3.ApplyByteDirectionToPosition(packet.Direction);

            var popItemSuccess = await player.PopSelectedItemStackAsync();

            if (!popItemSuccess)
            {
                var actualBlock = await context.Server.World.GetBlockAsync(position);
                await context.RemoteClient.SendPacketAsync(new BlockUpdatePacket
                {
                    X = position.X,
                    Y = (sbyte) position.Y,
                    Z = position.Z,
                    BlockId = actualBlock.BlockId,
                    Metadata = actualBlock.Metadata
                });
                return;
            }

            if (packet.ItemId is ItemId.TorchBlock)
            {
                await PlaceTorchAsync(packet, context);
            }
            else
            {
                if (popItemSuccess)
                {
                    await context.Server.World.SetBlockAsync(position, (BlockId) packet.ItemId, (byte) packet.Metadata!);
                }
                else
                {
                    await context.Server.World.SetBlockAsync(position, 0);
                }
            }
        }
        else
        {
            if (context.Server.Configuration.Debug)
            {
                await context.RemoteClient.SendChatAsync($"Position: {packet.X} {packet.Y} {packet.Z}");
            }
        }
    }

    private async Task PlaceTorchAsync(PlayerBlockPlacementPacket packet, ClientPacketHandlerContext context)
    {
        var position = packet.PositionAsVector3.ApplyByteDirectionToPosition(packet.Direction);

        // Torches can't be placed at the bottom of a block
        // TODO Actually they can if there is no block at Y-1 and a block at Y-1 and (X-1 or X+1 or Z-1 or Z+1)
        if (packet.Direction == 0)
        {
            //TODO Give back the item to the player
            await CancelPlacementAsync(position, context);
            return;
        }

        var orientationMetadata = (byte) packet.Direction;
        await context.Server.World.SetBlockAsync(position, BlockId.Torch, orientationMetadata);
    }

    private async Task PlaceSignAsync(PlayerBlockPlacementPacket packet, ClientPacketHandlerContext context)
    {
        var position = packet.PositionAsVector3.ApplyByteDirectionToPosition(packet.Direction);

        // Signs can't be placed at the bottom of a block
        if (packet.Direction == 0)
        {
            //TODO Give back the item to the player
            await CancelPlacementAsync(position, context);
            return;
        }

        var onFloor = packet.Direction == 1;
        var orientationMetadata = (byte) packet.Direction;
        if (onFloor)
        {
            var rotation = context.RemoteClient.Player!.Yaw + 180 % 360;
            if (rotation < 0)
                rotation += 360;
            orientationMetadata = (byte) (rotation / 22.5);
        }

        var blockId = onFloor ? BlockId.FloorSign : BlockId.WallSign;

        await context.Server.World.SetBlockAsync(position, blockId, orientationMetadata);

        var signTileEntity = new SignTileEntity(Chunk.WorldToLocal(position));
        await context.Server.World.SetTileEntityAsync(position, signTileEntity);
    }

    private async Task PlaceDoorAsync(PlayerBlockPlacementPacket packet, ClientPacketHandlerContext context)
    {
        var onFloor = packet.Direction == 1;
        if (!onFloor)
            return;
        //TODO
        await CancelPlacementAsync(new Vector3<int>(packet.X, packet.Y + 1, packet.Z), context);
        await CancelPlacementAsync(new Vector3<int>(packet.X, packet.Y + 2, packet.Z), context);
        await context.RemoteClient.SendChatAsync("Doors are not supported yet!");
    }

    private async Task CancelPlacementAsync(Vector3<int> position, ClientPacketHandlerContext context)
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
}
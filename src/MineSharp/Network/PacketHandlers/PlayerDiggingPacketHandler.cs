using MineSharp.Content;
using MineSharp.Content.Items;
using MineSharp.Core;
using MineSharp.Entities;
using MineSharp.Network.Packets;

namespace MineSharp.Network.PacketHandlers;

public class PlayerDiggingPacketHandler : IClientPacketHandler<PlayerDiggingPacket>
{
    public async Task HandleAsync(PlayerDiggingPacket packet, ClientPacketHandlerContext context)
    {
        var block = await context.Server.World.GetBlockAsync(packet.PositionAsVector3i);

        if (block.BlockId is BlockId.Air)
            return;

        var blockInfo = BlockInfoProvider.Get(block.BlockId);

        var miningItem = context.RemoteClient.Player!.HoldItemStack.ItemId;
        var miningItemInfo = miningItem != ItemId.Empty ? ItemInfoProvider.Get(miningItem) : null;

        if (packet.Status is PlayerDiggingStatus.Finished)
        {
            await context.Server.World.SetBlockAsync(packet.PositionAsVector3i, 0);
            await GeneratePickupItemAsync(block, packet.PositionAsVector3i, miningItemInfo, context);
            if (blockInfo.HasTileEntity)
                await context.Server.World.SetTileEntityAsync(packet.PositionAsVector3i, null);
        }
        else
        {
            if (packet.Status is PlayerDiggingStatus.Started && blockInfo.IsInstantDig(miningItemInfo, block.Metadata))
            {
                await context.Server.World.SetBlockAsync(packet.PositionAsVector3i, 0);
                await GeneratePickupItemAsync(block, packet.PositionAsVector3i, miningItemInfo, context);
                if (blockInfo.HasTileEntity)
                    await context.Server.World.SetTileEntityAsync(packet.PositionAsVector3i, null);
            }
        }
    }

    private async Task GeneratePickupItemAsync(Block block, Vector3i blockPosition, ItemInfo? miningItemInfo,
        ClientPacketHandlerContext context)
    {
        var blockInfo = BlockInfoProvider.Get(block.BlockId);
        var droppedItems = blockInfo.GetDroppedItems(miningItemInfo, block.Metadata);
        foreach (var droppedItem in droppedItems)
        {
            await GeneratePickupItemAsync(droppedItem, blockPosition, context);
        }
    }

    private async Task GeneratePickupItemAsync(ItemStack itemStack, Vector3i blockPosition, ClientPacketHandlerContext context)
    {
        var pickupItem = new PickupItem(context.Server, TimeSpan.FromSeconds(10))
        {
            Item = itemStack,
            Pitch = 0,
            Roll = 0,
            Rotation = 0,
            AbsolutePosition = blockPosition.ToAbsolutePosition() + new Vector3i(16)
        };
        context.Server.EntityManager.RegisterEntity(pickupItem);
        await context.Server.BroadcastPacketAsync(new PickupSpawnPacket(pickupItem));

        //TODO Handle this correctly
        context.Server.Looper.Schedule(TimeSpan.FromSeconds(4), async _ =>
        {
            if (context.Server.EntityManager.EntityExists(context.RemoteClient.Player!.EntityId))
            {
                await context.Server.BroadcastPacketAsync(new CollectItemPacket
                {
                    CollectedEntityId = pickupItem.EntityId,
                    CollectorEntityId = context.RemoteClient.Player!.EntityId
                });
            }

            await context.Server.BroadcastPacketAsync(new DestroyEntityPacket
            {
                EntityId = pickupItem.EntityId
            });
            context.Server.EntityManager.FreeEntity(pickupItem);
        });
    }
}
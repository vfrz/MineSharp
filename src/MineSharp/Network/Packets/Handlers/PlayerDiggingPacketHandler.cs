using MineSharp.Content;
using MineSharp.Core;
using MineSharp.Entities;

namespace MineSharp.Network.Packets.Handlers;

public class PlayerDiggingPacketHandler : IClientPacketHandler<PlayerDiggingPacket>
{
    // Sometime client doesn't send a packet with Finished status for some blocks
    private static readonly BlockId[] InstantDigBlocks =
    [
        BlockId.Sapling,
        BlockId.TallGrass,
        BlockId.DeadShrub,
        BlockId.YellowFlower,
        BlockId.RedFlower,
        BlockId.BrownMushroom,
        BlockId.RedMushroom,
        BlockId.Torch,
        BlockId.Wheat,
        BlockId.RedstoneTorch,
        BlockId.GlowingRedstoneTorch,
        BlockId.SugarCane,
        BlockId.Sapling
    ];

    public async Task HandleAsync(PlayerDiggingPacket packet, ClientPacketHandlerContext context)
    {
        var block = await context.Server.World.GetBlockAsync(packet.PositionAsVector3i);

        if (packet.Status is PlayerDiggingStatus.Finished)
        {
            await context.Server.World.SetBlockAsync(packet.PositionAsVector3i, 0);
            await GeneratePickupItemAsync(block, packet.PositionAsVector3i, context);
        }
        else
        {
            if (packet.Status is PlayerDiggingStatus.Started or PlayerDiggingStatus.Finished
                && InstantDigBlocks.Contains(await context.Server.World.GetBlockIdAsync(packet.PositionAsVector3i)))
            {
                await context.Server.World.SetBlockAsync(packet.PositionAsVector3i, 0);
                await GeneratePickupItemAsync(block, packet.PositionAsVector3i, context);
            }
        }
    }

    private async Task GeneratePickupItemAsync(Block block, Vector3i blockPosition, ClientPacketHandlerContext context)
    {
        var blockInfo = BlockInfoProvider.Get(block.BlockId);
        var miningItem = context.RemoteClient.Player!.HoldItemStack.ItemId;
        var droppedItems = blockInfo.GetDroppedItems(miningItem, block.Metadata);
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
using MineSharp.Blocks;
using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Entities;
using MineSharp.Items;

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
        var blockId = await context.Server.World.GetBlockIdAsync(packet.PositionAsVector3i);

        if (packet.Status is PlayerDiggingStatus.Finished)
        {
            await context.Server.World.SetBlockAsync(packet.PositionAsVector3i, 0);
            await GeneratePickupItemAsync(blockId, packet.PositionAsVector3i, context);
        }
        else
        {
            if (packet.Status is PlayerDiggingStatus.Started or PlayerDiggingStatus.Finished
                && InstantDigBlocks.Contains(await context.Server.World.GetBlockIdAsync(packet.PositionAsVector3i)))
            {
                await context.Server.World.SetBlockAsync(packet.PositionAsVector3i, 0);
                await GeneratePickupItemAsync(blockId, packet.PositionAsVector3i, context);
            }
        }
    }

    private async Task GeneratePickupItemAsync(BlockId blockId, Vector3i blockPosition,
        ClientPacketHandlerContext context)
    {
        var pickupItem = new PickupItem(TimeSpan.FromSeconds(10))
        {
            ItemId = (ItemId)blockId,
            Count = 1,
            PickupMetadata = 0,
            Pitch = 0,
            Roll = 0,
            Rotation = 0,
            AbsoluteX = blockPosition.X.ToAbsoluteInt() + 16,
            AbsoluteY = blockPosition.Y.ToAbsoluteInt() + 16,
            AbsoluteZ = blockPosition.Z.ToAbsoluteInt() + 16
        };
        context.Server.EntityManager.RegisterEntity(pickupItem);
        await context.Server.BroadcastPacketAsync(new PickupSpawnPacket
        {
            EntityId = pickupItem.EntityId,
            ItemId = pickupItem.ItemId,
            Count = pickupItem.Count,
            Metadata = pickupItem.PickupMetadata,
            AbsoluteX = pickupItem.AbsoluteX,
            AbsoluteY = pickupItem.AbsoluteY,
            AbsoluteZ = pickupItem.AbsoluteZ,
            Rotation = pickupItem.Rotation,
            Pitch = pickupItem.Pitch,
            Roll = pickupItem.Roll
        });
        context.Server.Looper.Schedule(TimeSpan.FromSeconds(0.5), async _ =>
        {
            if (context.Server.EntityManager.EntityExists(context.RemoteClient.Player!.EntityId))
            {
                await context.Server.BroadcastPacketAsync(new CollectItemPacket
                {
                    CollectedEntityId = pickupItem.EntityId,
                    CollectorEntityId = context.RemoteClient.Player!.EntityId
                });
                await context.Server.BroadcastPacketAsync(new DestroyEntityPacket
                {
                    EntityId = pickupItem.EntityId
                });
                context.Server.EntityManager.FreeEntity(pickupItem);
            }
        });
    }
}
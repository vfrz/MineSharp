using MineSharp.Content.Items;
using MineSharp.Core;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class TallGrassBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.TallGrassBlock;

    public override bool IsInstantDig(ItemInfo? miningItemInfo, byte blockMetadata) => true;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        // 12.5% chance of dropping a wheat seed
        if (ThreadSafeRandom.Shared.Next() <= 0.125)
        {
            return [new ItemStack(ItemId.WheatSeeds)];
        }

        return [];
    }
}
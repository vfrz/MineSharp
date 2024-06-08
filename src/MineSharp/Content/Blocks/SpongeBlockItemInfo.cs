using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class SpongeBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.SpongeBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.SpongeBlock)];
}
using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class StickyPistonBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.StickyPistonBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.StickyPistonBlock)];
}
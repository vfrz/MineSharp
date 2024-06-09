using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class PistonBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.PistonBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.PistonBlock)];
}
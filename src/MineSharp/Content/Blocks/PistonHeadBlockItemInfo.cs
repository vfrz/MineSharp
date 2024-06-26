using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class PistonHeadBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.PistonHeadBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.PistonBlock)];
}
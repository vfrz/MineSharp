using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class CactusBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.CactusBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.CactusBlock)];
}
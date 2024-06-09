using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class RedFlowerBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.RedFlowerBlock;

    public override bool IsInstantDig(ItemInfo? miningItemInfo, byte blockMetadata) => true;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.RedFlowerBlock)];
}
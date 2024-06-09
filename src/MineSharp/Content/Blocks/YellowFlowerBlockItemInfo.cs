using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class YellowFlowerBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.YellowFlowerBlock;

    public override bool IsInstantDig(ItemInfo? miningItemInfo, byte blockMetadata) => true;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.YellowFlowerBlock)];
}
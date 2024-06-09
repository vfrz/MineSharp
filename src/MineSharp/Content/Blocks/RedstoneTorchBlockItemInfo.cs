using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class RedstoneTorchBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.RedstoneTorchBlock;

    public override bool IsInstantDig(ItemInfo? miningItemInfo, byte blockMetadata) => true;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.RedstoneTorchBlock)];
}
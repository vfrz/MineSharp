using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class TorchBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.TorchBlock;

    public override bool IsInstantDig(ItemInfo? miningItemInfo, byte blockMetadata) => true;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.TorchBlock)];

    public override byte Emission => 14;
}
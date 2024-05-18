using MineSharp.Items;

namespace MineSharp.Blocks.Infos.Blocks;

public class PlanksBlockInfo : BlockInfo
{
    public override BlockId Id => BlockId.Planks;

    public override ItemStack[] GetDroppedItem(ItemId miningItem, byte blockMetadata)
        => [new ItemStack(ItemId.PlanksBlock)];
}
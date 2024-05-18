using MineSharp.Items;

namespace MineSharp.Blocks.Infos.Blocks;

public class WoodBlockInfo: BlockInfo
{
    public override BlockId Id => BlockId.Wood;

    public override ItemStack[] GetDroppedItem(ItemId miningItem, byte blockMetadata)
        => [new ItemStack(ItemId.WoodBlock, Metadata: blockMetadata)];
}
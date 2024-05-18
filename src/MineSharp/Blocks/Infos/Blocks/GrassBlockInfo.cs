using MineSharp.Items;

namespace MineSharp.Blocks.Infos.Blocks;

public class GrassBlockInfo : BlockInfo
{
    public override BlockId Id => BlockId.Grass;

    public override ItemStack[] GetDroppedItem(ItemId miningItem, byte blockMetadata)
        => [new ItemStack(ItemId.DirtBlock)];
}
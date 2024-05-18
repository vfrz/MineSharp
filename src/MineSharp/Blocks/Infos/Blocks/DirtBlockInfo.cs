using MineSharp.Items;

namespace MineSharp.Blocks.Infos.Blocks;

public class DirtBlockInfo : BlockInfo
{
    public override BlockId Id => BlockId.Dirt;

    public override ItemStack[] GetDroppedItem(ItemId miningItem, byte blockMetadata)
        => [new ItemStack(ItemId.DirtBlock)];
}
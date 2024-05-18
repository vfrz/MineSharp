using MineSharp.Items;

namespace MineSharp.Blocks.Infos;

public abstract class BlockInfo
{
    public abstract BlockId Id { get; }

    public abstract ItemStack[] GetDroppedItem(ItemId miningItem, byte blockMetadata);
}
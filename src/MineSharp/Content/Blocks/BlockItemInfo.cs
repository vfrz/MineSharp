using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public abstract class BlockItemInfo : ItemInfo
{
    public BlockId BlockId => (BlockId) ItemId;

    public virtual ItemStack[] GetDroppedItems(ItemId miningItem, byte blockMetadata) => [];

    public override byte StackMax => 64;
}
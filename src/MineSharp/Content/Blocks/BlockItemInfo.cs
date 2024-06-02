using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public abstract class BlockItemInfo : ItemInfo
{
    public BlockId BlockId => (BlockId) ItemId;

    public virtual ItemStack[] GetDroppedItems(ItemId miningItem, byte blockMetadata) => [];

    public virtual bool InstantDig => false;

    public virtual bool HasTileEntity => false;

    public override byte StackMax => 64;
}
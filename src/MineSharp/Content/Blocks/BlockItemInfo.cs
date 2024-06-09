using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public abstract class BlockItemInfo : ItemInfo
{
    public BlockId BlockId => (BlockId) ItemId;

    public virtual ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata) => [];

    public virtual bool IsInstantDig(ItemInfo? miningItemInfo, byte blockMetadata) => false;

    public virtual bool HasTileEntity => false;

    public override byte StackMax => 64;
}
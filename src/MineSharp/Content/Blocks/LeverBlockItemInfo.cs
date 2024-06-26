using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class LeverBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.LeverBlock;
    
    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.LeverBlock)];
}
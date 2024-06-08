using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class GrassBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.GrassBlock;
    
    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.DirtBlock)];
}
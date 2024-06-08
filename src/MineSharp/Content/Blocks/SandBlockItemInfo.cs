using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class SandBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.SandBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.SandBlock)];
}
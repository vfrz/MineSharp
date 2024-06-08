using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class SaplingBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.SaplingBlock;

    public override bool IsInstantDig(ItemInfo? miningItemInfo, byte blockMetadata) => true;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.SaplingBlock, Metadata: blockMetadata)];
}
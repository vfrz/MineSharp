using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class DetectorRailBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.DetectorRailBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.DetectorRailBlock)];
}
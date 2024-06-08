using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class PoweredRedstoneRepeaterBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.PoweredRedstoneRepeaterBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.RedstoneRepeater)];
}
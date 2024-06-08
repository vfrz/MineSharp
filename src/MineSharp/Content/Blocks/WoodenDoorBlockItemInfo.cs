using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class WoodenDoorBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.WoodenDoorBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.WoodenDoor)];
}
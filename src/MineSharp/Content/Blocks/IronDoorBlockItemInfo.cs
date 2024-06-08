using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class IronDoorBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.IronDoorBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.IronDoor)];
        return [];
    }
}
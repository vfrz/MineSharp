using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class DispenserBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.DispenserBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.DispenserBlock)];
        return [];
    }
}
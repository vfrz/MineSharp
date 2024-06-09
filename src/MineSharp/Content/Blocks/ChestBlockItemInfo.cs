using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class ChestBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.ChestBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        //TODO Handle items inside
        return [new ItemStack(ItemId.ChestBlock)];
    }
}
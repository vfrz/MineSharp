using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class NetherrackBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.NetherrackBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.NetherrackBlock)];
        return [];
    }
}
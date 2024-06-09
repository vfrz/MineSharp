using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class CobwebBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.CobwebBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is SwordItemInfo)
            return [new ItemStack(ItemId.String)];
        return [];
    }
}
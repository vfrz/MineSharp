using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class SugarCaneBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.SugarCaneBlock;

    public override bool IsInstantDig(ItemInfo? miningItemInfo, byte blockMetadata) => true;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.SugarCane)];
}
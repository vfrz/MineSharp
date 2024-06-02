using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class SoulSandBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.SoulSandBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.SoulSandBlock)];
}
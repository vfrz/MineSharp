using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class NoteBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.NoteBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.NoteBlock)];
}
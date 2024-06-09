using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class JukeboxBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.JukeboxBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        //TODO Handle disc drop if present inside
        return [new ItemStack(ItemId.JukeboxBlock)];
    }
}
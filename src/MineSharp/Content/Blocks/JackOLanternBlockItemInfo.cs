using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class JackOLanternBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.JackOLanternBlock;


    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.JackOLanternBlock)];
}
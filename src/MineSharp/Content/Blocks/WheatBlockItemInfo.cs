using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class WheatBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.WheatBlock;

    public override bool IsInstantDig(ItemInfo? miningItemInfo, byte blockMetadata) => true;
}
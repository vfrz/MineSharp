using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class DeadShrubBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.DeadShrubBlock;

    public override bool IsInstantDig(ItemInfo? miningItemInfo, byte blockMetadata) => true;
}
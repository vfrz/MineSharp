using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class DeadShrubBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.DeadShrubBlock;

    public override bool IsInstantDig(ItemInfo? miningItemInfo, byte blockMetadata) => true;
}
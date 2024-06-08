using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class WheatBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.WheatBlock;

    public override bool IsInstantDig(ItemInfo? miningItemInfo, byte blockMetadata) => true;
}
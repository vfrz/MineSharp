using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class LavaBucketItemInfo : ItemInfo
{
    public override ItemId ItemId => ItemId.LavaBucket;

    public override byte StackMax => 1;
}
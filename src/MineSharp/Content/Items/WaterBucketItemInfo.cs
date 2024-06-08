using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class WaterBucketItemInfo : ItemInfo
{
    public override ItemId ItemId => ItemId.WaterBucket;

    public override byte StackMax => 1;
}
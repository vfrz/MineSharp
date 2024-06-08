using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class BoatItemInfo : ItemInfo
{
    public override ItemId ItemId => ItemId.Boat;

    public override byte StackMax => 1;
}
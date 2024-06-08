using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class ChestMinecartItemInfo : ItemInfo
{
    public override ItemId ItemId => ItemId.ChestMinecart;

    public override byte StackMax => 1;
}
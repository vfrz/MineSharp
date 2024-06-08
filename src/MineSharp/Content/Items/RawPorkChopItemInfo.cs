using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class RawPorkChopItemInfo : FoodItemInfo
{
    public override ItemId ItemId => ItemId.RawPorkChop;
    public override short HealthRestore => 3;
}
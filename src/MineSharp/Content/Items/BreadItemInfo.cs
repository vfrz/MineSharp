using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class BreadItemInfo : FoodItemInfo
{
    public override ItemId ItemId => ItemId.Bread;
    public override short HealthRestore => 5;
}
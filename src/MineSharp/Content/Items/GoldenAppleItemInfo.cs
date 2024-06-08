using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class GoldenAppleItemInfo : FoodItemInfo
{
    public override ItemId ItemId => ItemId.GoldenApple;
    public override short HealthRestore => 20;
}
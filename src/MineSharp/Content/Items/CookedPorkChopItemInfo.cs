using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class CookedPorkChopItemInfo : FoodItemInfo
{
    public override ItemId ItemId => ItemId.CookedPorkChop;
    public override short HealthRestore => 8;
}
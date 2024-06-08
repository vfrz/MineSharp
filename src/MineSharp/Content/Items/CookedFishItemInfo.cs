using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class CookedFishItemInfo : FoodItemInfo
{
    public override ItemId ItemId => ItemId.CookedFish;
    public override short HealthRestore => 5;
}
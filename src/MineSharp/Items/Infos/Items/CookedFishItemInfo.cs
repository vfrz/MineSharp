namespace MineSharp.Items.Infos.Items;

public class CookedFishItemInfo : FoodItemInfo
{
    public override ItemId Id => ItemId.CookedFish;
    public override short HealthRestore { get; } //TODO
}
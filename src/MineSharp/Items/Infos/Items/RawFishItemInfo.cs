namespace MineSharp.Items.Infos.Items;

public class RawFishItemInfo : FoodItemInfo
{
    public override ItemId Id => ItemId.RawFish;
    public override short HealthRestore { get; } //TODO
}
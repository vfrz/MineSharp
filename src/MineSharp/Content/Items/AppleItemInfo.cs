namespace MineSharp.Content.Items;

public class AppleItemInfo : FoodItemInfo
{
    public override ItemId ItemId => ItemId.Apple;
    public override short HealthRestore => 4; //TODO Check
}
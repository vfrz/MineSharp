namespace MineSharp.Items.Infos.Items;

public class BreadItemInfo : FoodItemInfo
{
    public override ItemId Id => ItemId.Bread;
    public override short HealthRestore { get; } //TODO
}
namespace MineSharp.Items.Infos.Items;

public class GoldenAppleItemInfo : FoodItemInfo
{
    public override ItemId Id => ItemId.GoldenApple;
    public override short HealthRestore { get; } //TODO
}
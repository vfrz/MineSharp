namespace MineSharp.Content.Items;

public abstract class FoodItemInfo : ItemInfo
{
    public abstract short HealthRestore { get; }
    public override short DamageOnEntity => 1;
    public override byte StackMax => 1;
}
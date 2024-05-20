namespace MineSharp.Items.Infos;

public abstract class FoodItemInfo : ItemInfo
{
    public abstract short HealthRestore { get; }
    public override short DamageOnEntity => 1; //TODO Check
    public override byte StackMax => 1;
}
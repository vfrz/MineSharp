namespace MineSharp.Content.Items;

public abstract class ToolItemInfo : ItemInfo
{
    public abstract short Durability { get; }
    public abstract override short DamageOnEntity { get; }
    public override byte StackMax => 1;
}
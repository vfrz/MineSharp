namespace MineSharp.Content.Items;

public class WoodenAxeItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.WoodenAxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
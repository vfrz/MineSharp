namespace MineSharp.Content.Items;

public class WoodenShovelItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.WoodenShovel;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
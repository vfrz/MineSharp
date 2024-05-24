namespace MineSharp.Content.Items;

public class WoodenPickaxeItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.WoodenPickaxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
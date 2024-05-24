namespace MineSharp.Content.Items;

public class WoodenSwordItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.WoodenSword;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
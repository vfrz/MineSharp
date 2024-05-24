namespace MineSharp.Content.Items;

public class WoodenHoeItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.WoodenHoe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
namespace MineSharp.Content.Items;

public class GoldenPickaxeItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.GoldenPickaxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
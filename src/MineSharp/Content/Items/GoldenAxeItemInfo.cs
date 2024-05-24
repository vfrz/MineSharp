namespace MineSharp.Content.Items;

public class GoldenAxeItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.GoldenAxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
namespace MineSharp.Content.Items;

public class GoldenShovelItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.GoldenShovel;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
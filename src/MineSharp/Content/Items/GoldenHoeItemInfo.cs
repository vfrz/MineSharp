namespace MineSharp.Content.Items;

public class GoldenHoeItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.GoldenHoe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
namespace MineSharp.Content.Items;

public class DiamondHoeItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.DiamondHoe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
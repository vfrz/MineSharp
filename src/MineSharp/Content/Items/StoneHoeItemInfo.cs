namespace MineSharp.Content.Items;

public class StoneHoeItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.StoneHoe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
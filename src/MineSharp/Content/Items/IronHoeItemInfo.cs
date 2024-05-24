namespace MineSharp.Content.Items;

public class IronHoeItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.IronHoe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
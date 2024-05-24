namespace MineSharp.Content.Items;

public class IronAxeItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.IronAxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
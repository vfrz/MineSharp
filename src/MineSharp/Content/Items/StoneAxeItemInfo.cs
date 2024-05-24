namespace MineSharp.Content.Items;

public class StoneAxeItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.StoneAxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
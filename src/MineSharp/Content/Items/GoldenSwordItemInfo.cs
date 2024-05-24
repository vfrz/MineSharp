namespace MineSharp.Content.Items;

public class GoldenSwordItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.GoldenSword;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
namespace MineSharp.Content.Items;

public class StoneShovelItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.StoneShovel;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
namespace MineSharp.Content.Items;

public class StoneSwordItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.StoneSword;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
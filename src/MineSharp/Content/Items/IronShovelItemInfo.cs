namespace MineSharp.Content.Items;

public class IronShovelItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.IronShovel;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
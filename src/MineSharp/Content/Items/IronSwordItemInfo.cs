namespace MineSharp.Content.Items;

public class IronSwordItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.IronSword;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
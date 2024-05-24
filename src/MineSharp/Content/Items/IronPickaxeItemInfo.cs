namespace MineSharp.Content.Items;

public class IronPickaxeItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.IronPickaxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
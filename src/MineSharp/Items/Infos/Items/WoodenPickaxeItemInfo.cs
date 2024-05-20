namespace MineSharp.Items.Infos.Items;

public class WoodenPickaxeItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.WoodenPickaxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
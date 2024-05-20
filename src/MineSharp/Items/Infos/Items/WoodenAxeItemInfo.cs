namespace MineSharp.Items.Infos.Items;

public class WoodenAxeItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.WoodenAxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
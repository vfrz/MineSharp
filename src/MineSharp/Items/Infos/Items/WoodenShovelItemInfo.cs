namespace MineSharp.Items.Infos.Items;

public class WoodenShovelItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.WoodenShovel;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
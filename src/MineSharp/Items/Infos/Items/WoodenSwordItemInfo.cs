namespace MineSharp.Items.Infos.Items;

public class WoodenSwordItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.WoodenSword;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
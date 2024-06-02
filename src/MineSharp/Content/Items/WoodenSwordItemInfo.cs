namespace MineSharp.Content.Items;

public class WoodenSwordItemInfo : SwordItemInfo
{
    public override ItemId ItemId => ItemId.WoodenSword;
    public override short DamageOnEntity => 5;

    public override short Durability => 60;

    protected override ToolMaterial Material => ToolMaterial.Wood;
}
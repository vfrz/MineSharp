namespace MineSharp.Content.Items;

public class WoodenShovelItemInfo : ShovelItemInfo
{
    public override ItemId ItemId => ItemId.WoodenShovel;

    public override short DamageOnEntity => 2;

    public override short Durability => 60;

    public override ToolMaterial Material => ToolMaterial.Wood;
}
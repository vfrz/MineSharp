namespace MineSharp.Content.Items;

public class WoodenAxeItemInfo : AxeItemInfo
{
    public override ItemId ItemId => ItemId.WoodenAxe;

    public override short DamageOnEntity => 4;

    public override short Durability => 60;

    protected override ToolMaterial Material => ToolMaterial.Wood;
}
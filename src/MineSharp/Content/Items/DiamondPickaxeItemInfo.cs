namespace MineSharp.Content.Items;

public class DiamondPickaxeItemInfo : PickaxeItemInfo
{
    public override ItemId ItemId => ItemId.DiamondPickaxe;

    public override short DamageOnEntity => 6;

    public override short Durability => 1562;

    protected override ToolMaterial Material => ToolMaterial.Diamond;
}
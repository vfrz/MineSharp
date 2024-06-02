namespace MineSharp.Content.Items;

public class DiamondShovelItemInfo : ShovelItemInfo
{
    public override ItemId ItemId => ItemId.DiamondShovel;

    public override short DamageOnEntity => 5;

    public override short Durability => 1562;

    protected override ToolMaterial Material => ToolMaterial.Diamond;
}
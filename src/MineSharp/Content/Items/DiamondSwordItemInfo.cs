namespace MineSharp.Content.Items;

public class DiamondSwordItemInfo : SwordItemInfo
{
    public override ItemId ItemId => ItemId.DiamondSword;

    public override short DamageOnEntity => 11;

    public override short Durability => 1561;

    protected override ToolMaterial Material => ToolMaterial.Diamond;
}
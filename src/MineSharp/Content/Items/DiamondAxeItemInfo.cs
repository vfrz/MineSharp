namespace MineSharp.Content.Items;

public class DiamondAxeItemInfo : AxeItemInfo
{
    public override ItemId ItemId => ItemId.DiamondAxe;

    public override short DamageOnEntity => 7;

    public override short Durability => 1562;

    protected override ToolMaterial Material => ToolMaterial.Diamond;
}
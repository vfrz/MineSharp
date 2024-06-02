namespace MineSharp.Content.Items;

public class DiamondAxeItemInfo : AxeItemInfo
{
    public override ItemId ItemId => ItemId.DiamondAxe;

    public override short DamageOnEntity => 7;

    public override short Durability => 1562;

    public override ToolMaterial Material => ToolMaterial.Diamond;
}
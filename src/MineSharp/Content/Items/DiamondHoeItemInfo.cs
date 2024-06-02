namespace MineSharp.Content.Items;

public class DiamondHoeItemInfo : HoeItemInfo
{
    public override ItemId ItemId => ItemId.DiamondHoe;

    public override short DamageOnEntity => 2;

    public override short Durability => 1562;

    protected override ToolMaterial Material => ToolMaterial.Diamond;
}
namespace MineSharp.Content.Items;

public class GoldenAxeItemInfo : AxeItemInfo
{
    public override ItemId ItemId => ItemId.GoldenAxe;

    public override short DamageOnEntity => 4;

    public override short Durability => 33;

    protected override ToolMaterial Material => ToolMaterial.Gold;
}
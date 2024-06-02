namespace MineSharp.Content.Items;

public class GoldenShovelItemInfo : ShovelItemInfo
{
    public override ItemId ItemId => ItemId.GoldenShovel;

    public override short DamageOnEntity => 2;

    public override short Durability => 33;

    protected override ToolMaterial Material => ToolMaterial.Gold;
}
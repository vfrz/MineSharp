namespace MineSharp.Content.Items;

public class GoldenHoeItemInfo : HoeItemInfo
{
    public override ItemId ItemId => ItemId.GoldenHoe;

    public override short DamageOnEntity => 2;

    public override short Durability => 33;

    protected override ToolMaterial Material => ToolMaterial.Gold;
}
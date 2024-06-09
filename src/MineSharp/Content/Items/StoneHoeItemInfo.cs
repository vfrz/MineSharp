namespace MineSharp.Content.Items;

public class StoneHoeItemInfo : HoeItemInfo
{
    public override ItemId ItemId => ItemId.StoneHoe;

    public override short DamageOnEntity => 2;

    public override short Durability => 132;

    public override ToolMaterial Material => ToolMaterial.Stone;
}
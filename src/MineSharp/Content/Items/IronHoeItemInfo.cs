namespace MineSharp.Content.Items;

public class IronHoeItemInfo : HoeItemInfo
{
    public override ItemId ItemId => ItemId.IronHoe;

    public override short DamageOnEntity => 2;

    public override short Durability => 251;

    public override ToolMaterial Material => ToolMaterial.Iron;
}
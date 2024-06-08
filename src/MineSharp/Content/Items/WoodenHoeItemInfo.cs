using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class WoodenHoeItemInfo : HoeItemInfo
{
    public override ItemId ItemId => ItemId.WoodenHoe;

    public override short DamageOnEntity => 2;

    public override short Durability => 60;

    public override ToolMaterial Material => ToolMaterial.Wood;
}
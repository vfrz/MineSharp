using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class WoodenPickaxeItemInfo : PickaxeItemInfo
{
    public override ItemId ItemId => ItemId.WoodenPickaxe;

    public override short DamageOnEntity => 3;

    public override short Durability => 60;

    public override ToolMaterial Material => ToolMaterial.Wood;
}
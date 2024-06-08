using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class GoldenPickaxeItemInfo : PickaxeItemInfo
{
    public override ItemId ItemId => ItemId.GoldenPickaxe;

    public override short DamageOnEntity => 3;

    public override short Durability => 33;

    public override ToolMaterial Material => ToolMaterial.Gold;
}
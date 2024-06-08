using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class StoneShovelItemInfo : ShovelItemInfo
{
    public override ItemId ItemId => ItemId.StoneShovel;

    public override short DamageOnEntity => 3;

    public override short Durability => 132;

    public override ToolMaterial Material => ToolMaterial.Stone;
}
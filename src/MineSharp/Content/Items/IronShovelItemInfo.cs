using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class IronShovelItemInfo : ShovelItemInfo
{
    public override ItemId ItemId => ItemId.IronShovel;

    public override short DamageOnEntity => 4;

    public override short Durability => 251;

    public override ToolMaterial Material => ToolMaterial.Iron;
}
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class StoneAxeItemInfo : AxeItemInfo
{
    public override ItemId ItemId => ItemId.StoneAxe;

    public override short DamageOnEntity => 5;

    public override short Durability => 132;

    public override ToolMaterial Material => ToolMaterial.Stone;
}
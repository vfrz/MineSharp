using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class DiamondLeggingsItemInfo : ArmorItemInfo
{
    public override ItemId ItemId => ItemId.DiamondLeggings;
    public override short DefensePoints { get; } //TODO
}
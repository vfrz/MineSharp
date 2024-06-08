using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class DiamondBootsItemInfo : ArmorItemInfo
{
    public override ItemId ItemId => ItemId.DiamondBoots;
    public override short DefensePoints { get; } //TODO
}
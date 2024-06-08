using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class GoldenLeggingsItemInfo : ArmorItemInfo
{
    public override ItemId ItemId => ItemId.GoldenLeggings;
    public override short DefensePoints { get; } //TODO
}
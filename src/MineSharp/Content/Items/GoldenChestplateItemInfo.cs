using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class GoldenChestplateItemInfo : ArmorItemInfo
{
    public override ItemId ItemId => ItemId.GoldenChestplate;
    public override short DefensePoints { get; } //TODO
}
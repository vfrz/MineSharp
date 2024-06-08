using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class LeatherBootsItemInfo : ArmorItemInfo
{
    public override ItemId ItemId => ItemId.LeatherBoots;
    public override short DefensePoints { get; } //TODO
}
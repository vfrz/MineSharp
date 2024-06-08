using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class LeatherHelmetItemInfo : ArmorItemInfo
{
    public override ItemId ItemId => ItemId.LeatherHelmet;
    public override short DefensePoints { get; } //TODO
}
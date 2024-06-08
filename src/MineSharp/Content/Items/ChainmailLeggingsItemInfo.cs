using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class ChainmailLeggingsItemInfo : ArmorItemInfo
{
    public override ItemId ItemId => ItemId.ChainmailLeggings;
    public override short DefensePoints { get; } //TODO
}
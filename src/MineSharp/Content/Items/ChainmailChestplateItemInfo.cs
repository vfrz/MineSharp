using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class ChainmailChestplateItemInfo : ArmorItemInfo
{
    public override ItemId ItemId => ItemId.ChainmailChestplate;
    public override short DefensePoints { get; } //TODO
}
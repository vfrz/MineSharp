using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class ShearsItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.Shears;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class BowItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.Bow;
    public override short DamageOnEntity { get; } //TODO 
    public override short Durability { get; }
}
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class ArrowItemInfo : ItemInfo
{
    public override ItemId ItemId => ItemId.Arrow;
    public override short DamageOnEntity { get; } //TODO
    public override byte StackMax { get; }
}
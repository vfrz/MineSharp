namespace MineSharp.Content.Items;

public class FishingRodItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.FishingRod;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
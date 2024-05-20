namespace MineSharp.Items.Infos.Items;

public class FishingRodItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.FishingRod;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
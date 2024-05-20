namespace MineSharp.Items.Infos.Items;

public class GoldenHoeItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.GoldenHoe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
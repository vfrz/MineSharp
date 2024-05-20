namespace MineSharp.Items.Infos.Items;

public class DiamondHoeItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.DiamondHoe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
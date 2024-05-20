namespace MineSharp.Items.Infos.Items;

public class WoodenHoeItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.WoodenHoe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
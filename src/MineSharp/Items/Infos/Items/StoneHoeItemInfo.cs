namespace MineSharp.Items.Infos.Items;

public class StoneHoeItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.StoneHoe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
namespace MineSharp.Items.Infos.Items;

public class IronHoeItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.IronHoe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
namespace MineSharp.Items.Infos.Items;

public class IronAxeItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.IronAxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
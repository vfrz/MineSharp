namespace MineSharp.Items.Infos.Items;

public class BowItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.Bow;
    public override short DamageOnEntity { get; } //TODO 
    public override short Durability { get; }
}
namespace MineSharp.Items.Infos.Items;

public class ShearsItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.Shears;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
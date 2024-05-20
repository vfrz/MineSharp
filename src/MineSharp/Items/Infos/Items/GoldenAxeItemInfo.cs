namespace MineSharp.Items.Infos.Items;

public class GoldenAxeItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.GoldenAxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
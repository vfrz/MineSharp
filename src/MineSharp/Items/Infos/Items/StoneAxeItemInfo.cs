namespace MineSharp.Items.Infos.Items;

public class StoneAxeItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.StoneAxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
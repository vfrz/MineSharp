namespace MineSharp.Items.Infos.Items;

public class GoldenSwordItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.GoldenSword;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
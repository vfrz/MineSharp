namespace MineSharp.Items.Infos.Items;

public class GoldenShovelItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.GoldenShovel;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
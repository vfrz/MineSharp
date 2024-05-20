namespace MineSharp.Items.Infos.Items;

public class GoldenPickaxeItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.GoldenPickaxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
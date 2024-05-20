namespace MineSharp.Items.Infos.Items;

public class IronPickaxeItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.IronPickaxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
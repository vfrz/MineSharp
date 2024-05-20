namespace MineSharp.Items.Infos.Items;

public class IronShovelItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.IronShovel;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
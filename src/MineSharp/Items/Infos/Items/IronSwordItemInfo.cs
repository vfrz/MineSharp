namespace MineSharp.Items.Infos.Items;

public class IronSwordItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.IronSword;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
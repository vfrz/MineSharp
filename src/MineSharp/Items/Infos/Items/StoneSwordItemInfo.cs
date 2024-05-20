namespace MineSharp.Items.Infos.Items;

public class StoneSwordItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.StoneSword;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
namespace MineSharp.Items.Infos.Items;

public class StoneShovelItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.StoneShovel;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}
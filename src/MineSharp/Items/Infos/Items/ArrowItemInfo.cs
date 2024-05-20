namespace MineSharp.Items.Infos.Items;

public class ArrowItemInfo : ItemInfo
{
    public override ItemId Id => ItemId.Arrow;
    public override short DamageOnEntity { get; } //TODO
    public override byte StackMax { get; }
}
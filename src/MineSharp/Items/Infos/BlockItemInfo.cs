namespace MineSharp.Items.Infos;

public abstract class BlockItemInfo : ItemInfo
{
    public override byte StackMax => 64;

    public override short DamageOnEntity => 1;
}
namespace MineSharp.Items.Infos;

public abstract class ItemInfo
{
    public abstract ItemId Id { get; }

    public virtual short DamageOnEntity => 1;

    public virtual byte StackMax => 64;
}
namespace MineSharp.Content.Items;

public abstract class ItemInfo
{
    public abstract ItemId ItemId { get; }

    public virtual short DamageOnEntity => 1;

    public virtual byte StackMax => 64;
}
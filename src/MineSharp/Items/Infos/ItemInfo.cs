namespace MineSharp.Items.Infos;

public abstract class ItemInfo
{
    public abstract ItemId Id { get; }

    public abstract short DamageOnEntity { get; }

    public abstract byte StackMax { get; }
}
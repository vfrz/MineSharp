namespace MineSharp.Content.Items;

public abstract class ArmorItemInfo : ItemInfo
{
    public abstract short DefensePoints { get; }
    public override byte StackMax => 1;
}
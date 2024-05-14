namespace MineSharp.Items.Infos;

public abstract class ToolItemInfo : ItemInfo
{
    public abstract short Durability { get; }

    public override byte StackMax => 1;
}
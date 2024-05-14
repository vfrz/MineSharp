namespace MineSharp.Items.Infos;

public class PlaceholderItemInfo : ItemInfo
{
    public override ItemId Id { get; }
    public override short DamageOnEntity => 1;
    public override byte StackMax => 1;

    public PlaceholderItemInfo(ItemId id)
    {
        Id = id;
    }
}
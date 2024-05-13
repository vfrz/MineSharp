using System.Collections.Frozen;

namespace MineSharp.Items.Infos;

public abstract class ItemInfo
{
    public abstract ItemId Id { get; }

    public abstract short DamageOnEntity { get; }

    private static readonly IReadOnlyDictionary<ItemId, ItemInfo> Data = new Dictionary<ItemId, ItemInfo>
    {
        {ItemId.DiamondSword, new DiamondSwordItemInfo()}
    }.ToFrozenDictionary();

    public static ItemInfo Get(ItemId itemId)
    {
        if (Data.TryGetValue(itemId, out var itemInfo))
            return itemInfo;
        return new PlaceholderItemInfo(itemId);
    }
}
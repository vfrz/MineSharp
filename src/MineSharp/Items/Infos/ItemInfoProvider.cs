using System.Collections.Frozen;

namespace MineSharp.Items.Infos;

public static class ItemInfoProvider
{
    private static readonly FrozenDictionary<ItemId, ItemInfo> Data = typeof(ItemInfoProvider).Assembly.GetTypes()
        .Where(type => type.IsSubclassOf(typeof(ItemInfo)) && type is { IsAbstract: false, IsInterface: false })
        .Where(type => type != typeof(PlaceholderItemInfo)) //TODO Remove this when ready
        .Select(type => (ItemInfo)Activator.CreateInstance(type)!)
        .ToFrozenDictionary(itemInfo => itemInfo.Id);

    public static ItemInfo Get(ItemId itemId)
    {
        if (Data.TryGetValue(itemId, out var itemInfo))
            return itemInfo;
        return new PlaceholderItemInfo(itemId); //TODO Replace by exception when ready
    }
}
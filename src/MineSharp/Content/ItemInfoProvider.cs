using System.Collections.Frozen;
using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content;

public static class ItemInfoProvider
{
    private static readonly FrozenDictionary<ItemId, ItemInfo> Data = typeof(ItemInfoProvider).Assembly.GetTypes()
        .Where(type => type.IsSubclassOf(typeof(ItemInfo)) && type is {IsAbstract: false, IsInterface: false})
        .Select(type => (ItemInfo) Activator.CreateInstance(type)!)
        .ToFrozenDictionary(itemInfo => itemInfo.ItemId);

    public static IEnumerable<ItemInfo> All => Data.Values;

    public static ItemInfo Get(ItemId itemId)
    {
        if (Data.TryGetValue(itemId, out var itemInfo))
            return itemInfo;
        throw new Exception($"No item info for item id: {itemId}");
    }
}
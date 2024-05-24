using System.Collections.Frozen;
using MineSharp.Content.Blocks;

namespace MineSharp.Content;

public class BlockInfoProvider
{
    private static readonly FrozenDictionary<BlockId, BlockItemInfo> Data = typeof(BlockInfoProvider).Assembly.GetTypes()
        .Where(type => type.IsSubclassOf(typeof(BlockItemInfo)) && type is { IsAbstract: false, IsInterface: false })
        .Select(type => (BlockItemInfo)Activator.CreateInstance(type)!)
        .ToFrozenDictionary(blockInfo => blockInfo.BlockId);

    public static BlockItemInfo Get(BlockId blockId)
    {
        if (Data.TryGetValue(blockId, out var itemInfo))
            return itemInfo;
        throw new Exception($"No block info for block id: {blockId}");
    }
}
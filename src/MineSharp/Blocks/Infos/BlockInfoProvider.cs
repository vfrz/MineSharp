using System.Collections.Frozen;

namespace MineSharp.Blocks.Infos;

public class BlockInfoProvider
{
    private static readonly FrozenDictionary<BlockId, BlockInfo> Data = typeof(BlockInfoProvider).Assembly.GetTypes()
        .Where(type => type.IsSubclassOf(typeof(BlockInfo)) && type is { IsAbstract: false, IsInterface: false })
        .Where(type => type != typeof(PlaceholderBlockInfo)) //TODO Remove this when ready
        .Select(type => (BlockInfo)Activator.CreateInstance(type)!)
        .ToFrozenDictionary(blockInfo => blockInfo.Id);

    public static BlockInfo Get(BlockId blockId)
    {
        if (Data.TryGetValue(blockId, out var itemInfo))
            return itemInfo;
        return new PlaceholderBlockInfo(blockId); //TODO Replace by exception when ready
    }
}
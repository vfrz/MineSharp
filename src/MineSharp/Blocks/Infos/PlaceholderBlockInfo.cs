using MineSharp.Items;

namespace MineSharp.Blocks.Infos;

public class PlaceholderBlockInfo(BlockId id) : BlockInfo
{
    public override BlockId Id { get; } = id;

    public override ItemStack[] GetDroppedItem(ItemId miningItem, byte blockMetadata) => [];
}
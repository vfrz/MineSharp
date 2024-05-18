using MineSharp.Core;
using MineSharp.Items;

namespace MineSharp.Blocks.Infos.Blocks;

public class TallGrassBlockInfo : BlockInfo
{
    public override BlockId Id => BlockId.TallGrass;

    private readonly ThreadSafeRandom _random = new();

    public override ItemStack[] GetDroppedItem(ItemId miningItem, byte blockMetadata)
    {
        // 12.5% chance of dropping a wheat seed
        if (_random.Next() <= 0.125)
        {
            return [new ItemStack(ItemId.WheatSeeds)];
        }

        return [];
    }
}
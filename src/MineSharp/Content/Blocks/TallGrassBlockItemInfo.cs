using MineSharp.Core;

namespace MineSharp.Content.Blocks;

public class TallGrassBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.TallGrassBlock;
    
    private readonly ThreadSafeRandom _random = new();

    public override ItemStack[] GetDroppedItems(ItemId miningItem, byte blockMetadata)
    {
        // 12.5% chance of dropping a wheat seed
        if (_random.Next() <= 0.125)
        {
            return [new ItemStack(ItemId.WheatSeeds)];
        }

        return [];
    }
}
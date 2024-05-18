using MineSharp.Items;

namespace MineSharp.Blocks.Infos.Blocks;

public class StoneBlockInfo : BlockInfo
{
    public override BlockId Id => BlockId.Stone;

    private readonly ItemId[] _requiredTools =
    [
        ItemId.WoodenPickaxe,
        ItemId.StonePickaxe,
        ItemId.IronPickaxe,
        ItemId.GoldenPickaxe,
        ItemId.DiamondPickaxe
    ];

    public override ItemStack[] GetDroppedItem(ItemId miningItem, byte blockMetadata)
    {
        if (_requiredTools.Contains(miningItem))
        {
            return [new ItemStack(ItemId.CobblestoneBlock)];
        }

        return [];
    }
}
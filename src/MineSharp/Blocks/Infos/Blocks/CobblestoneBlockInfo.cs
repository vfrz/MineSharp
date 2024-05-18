using MineSharp.Items;

namespace MineSharp.Blocks.Infos.Blocks;

public class CobblestoneBlockInfo : BlockInfo
{
    public override BlockId Id => BlockId.Cobblestone;

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
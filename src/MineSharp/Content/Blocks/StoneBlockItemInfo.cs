namespace MineSharp.Content.Blocks;

public class StoneBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.StoneBlock;

    private readonly ItemId[] _requiredTools =
    [
        ItemId.WoodenPickaxe,
        ItemId.StonePickaxe,
        ItemId.IronPickaxe,
        ItemId.GoldenPickaxe,
        ItemId.DiamondPickaxe
    ];

    public override ItemStack[] GetDroppedItems(ItemId miningItem, byte blockMetadata)
    {
        if (_requiredTools.Contains(miningItem))
        {
            return [new ItemStack(ItemId.CobblestoneBlock)];
        }

        return [];
    }
}
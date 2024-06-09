using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class FurnaceBlockItemInfo : BlockItemInfo, ICraftable
{
    public override ItemId ItemId => ItemId.FurnaceBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.FurnaceBlock)];
        return [];
    }

    public CraftingRecipe CraftingRecipe => new(new[,]
    {
        { new(ItemId.CobblestoneBlock), new(ItemId.CobblestoneBlock), new(ItemId.CobblestoneBlock) },
        { new(ItemId.CobblestoneBlock), ItemStack.Empty, new(ItemId.CobblestoneBlock) },
        { new(ItemId.CobblestoneBlock), new(ItemId.CobblestoneBlock), new(ItemId.CobblestoneBlock) }
    }, new ItemStack(ItemId));
}
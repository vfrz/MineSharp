namespace MineSharp.Content.Blocks;

public class PlanksBlockItemInfo : BlockItemInfo, ICraftable
{
    public override ItemId ItemId => ItemId.PlanksBlock;

    public override ItemStack[] GetDroppedItems(ItemId miningItem, byte blockMetadata)
        => [new ItemStack(ItemId.PlanksBlock)];

    public CraftingRecipe CraftingRecipe => new(new ItemStack[,]
    {
        {new(ItemId.WoodBlock)}
    }, new ItemStack(ItemId, 4));
}
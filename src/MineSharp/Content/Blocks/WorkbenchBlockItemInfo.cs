using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class WorkbenchBlockItemInfo : BlockItemInfo, ICraftable
{
    public override ItemId ItemId => ItemId.WorkbenchBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.WorkbenchBlock)];

    public CraftingRecipe CraftingRecipe => new(new ItemStack[,]
    {
        { new(ItemId.PlanksBlock), new(ItemId.PlanksBlock) },
        { new(ItemId.PlanksBlock), new(ItemId.PlanksBlock) }
    }, new ItemStack(ItemId));
}
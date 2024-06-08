using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public abstract class ShovelItemInfo : MaterialToolItemInfo
{
    public override CraftingRecipe CraftingRecipe => new(new ItemStack[,]
    {
        { new(Material.ItemId) },
        { new(ItemId.Stick) },
        { new(ItemId.Stick) }
    }, new ItemStack(ItemId));
}
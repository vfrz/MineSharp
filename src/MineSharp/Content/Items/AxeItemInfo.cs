using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public abstract class AxeItemInfo : MaterialToolItemInfo
{
    public override CraftingRecipe CraftingRecipe => new(new[,]
    {
        { new(Material.ItemId), new(Material.ItemId) },
        { new(Material.ItemId), new(ItemId.Stick) },
        { ItemStack.Empty, new(ItemId.Stick) }
    }, new ItemStack(ItemId), Mirrored: true);
}
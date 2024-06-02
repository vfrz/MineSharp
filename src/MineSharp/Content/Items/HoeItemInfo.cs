namespace MineSharp.Content.Items;

public abstract class HoeItemInfo : MaterialToolItemInfo
{
    public override CraftingRecipe CraftingRecipe => new(new[,]
    {
        { new(Material.ItemId), new(Material.ItemId) },
        { ItemStack.Empty, new(ItemId.Stick) },
        { ItemStack.Empty, new(ItemId.Stick) }
    }, new ItemStack(ItemId), Mirrored: true);
}
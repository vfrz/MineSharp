namespace MineSharp.Content.Items;

public abstract class SwordItemInfo : MaterialToolItemInfo
{
    public override CraftingRecipe CraftingRecipe => new(new ItemStack[,]
    {
        { new(Material.ItemId) },
        { new(Material.ItemId) },
        { new(ItemId.Stick) }
    }, new ItemStack(ItemId));
}
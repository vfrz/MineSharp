namespace MineSharp.Content.Items;

public abstract class PickaxeItemInfo : MaterialToolItemInfo
{
    public override CraftingRecipe CraftingRecipe => new(new[,]
    {
        { new(Material.ItemId), new(Material.ItemId), new(Material.ItemId) },
        { ItemStack.Empty, new(ItemId.Stick), ItemStack.Empty },
        { ItemStack.Empty, new(ItemId.Stick), ItemStack.Empty }
    }, new ItemStack(ItemId));
}
namespace MineSharp.Content.Items;

public class DiamondPickaxeItemInfo : ToolItemInfo, ICraftable
{
    public override ItemId ItemId => ItemId.DiamondPickaxe;
    public override short DamageOnEntity => 5;
    public override short Durability => 1562;

    public CraftingRecipe CraftingRecipe => new(new[,]
    {
        {new(ItemId.Diamond), new(ItemId.Diamond), new(ItemId.Diamond)},
        {ItemStack.Empty, new(ItemId.Stick), ItemStack.Empty},
        {ItemStack.Empty, new(ItemId.Stick), ItemStack.Empty}
    }, new ItemStack(ItemId));
}
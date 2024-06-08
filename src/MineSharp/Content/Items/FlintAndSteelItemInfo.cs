using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class FlintAndSteelItemInfo : ToolItemInfo, ICraftable
{
    public override ItemId ItemId => ItemId.FlintAndSteel;

    public override short DamageOnEntity { get; } //TODO

    public override short Durability { get; }

    public CraftingRecipe CraftingRecipe => new(new[,]
    {
        { new(ItemId.IronIngot), ItemStack.Empty },
        { ItemStack.Empty, new(ItemId.Flint) }
    }, new ItemStack(ItemId), Mirrored: true);
}
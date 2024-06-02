namespace MineSharp.Content.Items;

public abstract class MaterialToolItemInfo : ToolItemInfo, ICraftable
{
    protected abstract ToolMaterial Material { get; }
    public abstract CraftingRecipe CraftingRecipe { get; }
}
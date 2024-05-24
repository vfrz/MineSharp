namespace MineSharp.Content;

public record CraftingRecipe(ItemStack[,] Pattern, ItemStack Output, bool MatchMetadata = false);
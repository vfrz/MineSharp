using MineSharp.Sdk.Core;

namespace MineSharp.Content;

public record CraftingRecipe(ItemStack[,] Pattern, ItemStack Output, bool MatchMetadata = false, bool Mirrored = false);
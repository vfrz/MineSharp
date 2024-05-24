namespace MineSharp.Content;

public static class CraftingSystem
{
    private static readonly CraftingRecipe[] Recipes = ItemInfoProvider.All
        .OfType<ICraftable>()
        .Select(craftable => craftable.CraftingRecipe)
        .ToArray();

    public static ItemStack Craft(ItemStack[,] inputGrid)
    {
        inputGrid = NormalizeGrid(inputGrid);

        foreach (var recipe in Recipes)
        {
            if (IsMatchingRecipe(inputGrid, recipe))
                return recipe.Output;
        }

        return ItemStack.Empty;
    }

    private static bool IsMatchingRecipe(ItemStack[,] inputGrid, CraftingRecipe recipe)
    {
        if (inputGrid.GetLength(0) != recipe.Pattern.GetLength(0) || inputGrid.GetLength(1) != recipe.Pattern.GetLength(1))
            return false;

        for (var i = 0; i < inputGrid.GetLength(0); i++)
        {
            for (var j = 0; j < inputGrid.GetLength(1); j++)
            {
                if (!inputGrid[i, j].Match(recipe.Pattern[i, j], matchMetadata: recipe.MatchMetadata))
                    return false;
            }
        }

        return true;
    }

    private static ItemStack[,] NormalizeGrid(ItemStack[,] grid)
    {
        int minX = grid.GetLength(0), minY = grid.GetLength(1), maxX = 0, maxY = 0;
        for (var x = 0; x < grid.GetLength(0); x++)
        {
            for (var y = 0; y < grid.GetLength(1); y++)
            {
                if (grid[x, y] != ItemStack.Empty)
                {
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }
        }

        var newWidth = maxX - minX + 1;
        var newHeight = maxY - minY + 1;
        var normalizedGrid = new ItemStack[newWidth, newHeight];

        for (var x = 0; x < newWidth; x++)
        {
            for (var y = 0; y < newHeight; y++)
            {
                normalizedGrid[x, y] = grid[minX + x, minY + y];
            }
        }

        return normalizedGrid;
    }
}
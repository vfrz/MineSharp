using FluentAssertions;
using MineSharp.Content;

namespace MineSharp.Tests.Content;

[TestClass]
public class CraftingSystemTests
{
    [TestMethod]
    public void CraftingSystem_Craft_Planks()
    {
        var output = CraftingSystem.Craft(new ItemStack[,] {{new(ItemId.WoodBlock)}});

        output.ItemId.Should().Be(ItemId.PlanksBlock);
        output.Count.Should().Be(4);
    }
    
    [TestMethod]
    public void CraftingSystem_Craft_DiamondPickaxe()
    {
        var output = CraftingSystem.Craft(new[,]
        {
            {new(ItemId.Diamond), new(ItemId.Diamond), new(ItemId.Diamond)},
            {ItemStack.Empty, new(ItemId.Stick), ItemStack.Empty},
            {ItemStack.Empty, new(ItemId.Stick), ItemStack.Empty}
        });

        output.ItemId.Should().Be(ItemId.DiamondPickaxe);
        output.Count.Should().Be(1);
    }
}
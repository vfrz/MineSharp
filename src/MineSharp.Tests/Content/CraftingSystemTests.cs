using FluentAssertions;
using MineSharp.Content;

namespace MineSharp.Tests.Content;

[TestClass]
public class CraftingSystemTests
{
    [TestMethod]
    public void CraftingSystem_Craft_Planks()
    {
        var output = CraftingSystem.Craft(new ItemStack[,] { { new(ItemId.WoodBlock) } });

        output.ItemId.Should().Be(ItemId.PlanksBlock);
        output.Count.Should().Be(4);
    }

    [TestMethod]
    public void CraftingSystem_Craft_FlintAndSteel()
    {
        var output = CraftingSystem.Craft(new[,]
        {
            { new(ItemId.IronIngot), ItemStack.Empty },
            { ItemStack.Empty, new(ItemId.Flint) }
        });

        output.ItemId.Should().Be(ItemId.FlintAndSteel);
        output.Count.Should().Be(1);
    }

    [TestMethod]
    public void CraftingSystem_Craft_FlintAndSteel_Mirrored()
    {
        var output = CraftingSystem.Craft(new[,]
        {
            { ItemStack.Empty, new(ItemId.IronIngot) },
            { new(ItemId.Flint), ItemStack.Empty }
        });

        output.ItemId.Should().Be(ItemId.FlintAndSteel);
        output.Count.Should().Be(1);
    }

    [DataTestMethod]
    [DataRow(ItemId.PlanksBlock, ItemId.WoodenPickaxe)]
    [DataRow(ItemId.GoldIngot, ItemId.GoldenPickaxe)]
    [DataRow(ItemId.CobblestoneBlock, ItemId.StonePickaxe)]
    [DataRow(ItemId.IronIngot, ItemId.IronPickaxe)]
    [DataRow(ItemId.Diamond, ItemId.DiamondPickaxe)]
    public void CraftingSystem_Craft_Pickaxes(ItemId material, ItemId expectedResult)
    {
        var output = CraftingSystem.Craft(new[,]
        {
            { new(material), new(material), new(material) },
            { ItemStack.Empty, new(ItemId.Stick), ItemStack.Empty },
            { ItemStack.Empty, new(ItemId.Stick), ItemStack.Empty }
        });

        output.ItemId.Should().Be(expectedResult);
        output.Count.Should().Be(1);
    }

    [DataTestMethod]
    [DataRow(ItemId.PlanksBlock, ItemId.WoodenSword)]
    [DataRow(ItemId.GoldIngot, ItemId.GoldenSword)]
    [DataRow(ItemId.CobblestoneBlock, ItemId.StoneSword)]
    [DataRow(ItemId.IronIngot, ItemId.IronSword)]
    [DataRow(ItemId.Diamond, ItemId.DiamondSword)]
    public void CraftingSystem_Craft_Swords(ItemId materialItemId, ItemId expectedResult)
    {
        var output = CraftingSystem.Craft(new[,]
        {
            { ItemStack.Empty, new(materialItemId), ItemStack.Empty },
            { ItemStack.Empty, new(materialItemId), ItemStack.Empty },
            { ItemStack.Empty, new(ItemId.Stick), ItemStack.Empty }
        });

        output.ItemId.Should().Be(expectedResult);
        output.Count.Should().Be(1);
    }

    [DataTestMethod]
    [DataRow(ItemId.PlanksBlock, ItemId.WoodenAxe)]
    [DataRow(ItemId.GoldIngot, ItemId.GoldenAxe)]
    [DataRow(ItemId.CobblestoneBlock, ItemId.StoneAxe)]
    [DataRow(ItemId.IronIngot, ItemId.IronAxe)]
    [DataRow(ItemId.Diamond, ItemId.DiamondAxe)]
    public void CraftingSystem_Craft_Axes(ItemId materialItemId, ItemId expectedResult)
    {
        var output = CraftingSystem.Craft(new[,]
        {
            { new(materialItemId), new(materialItemId), ItemStack.Empty },
            { new(materialItemId), new(ItemId.Stick), ItemStack.Empty },
            { ItemStack.Empty, new(ItemId.Stick), ItemStack.Empty }
        });

        output.ItemId.Should().Be(expectedResult);
        output.Count.Should().Be(1);
    }

    [DataTestMethod]
    [DataRow(ItemId.PlanksBlock, ItemId.WoodenShovel)]
    [DataRow(ItemId.GoldIngot, ItemId.GoldenShovel)]
    [DataRow(ItemId.CobblestoneBlock, ItemId.StoneShovel)]
    [DataRow(ItemId.IronIngot, ItemId.IronShovel)]
    [DataRow(ItemId.Diamond, ItemId.DiamondShovel)]
    public void CraftingSystem_Craft_Shovels(ItemId materialItemId, ItemId expectedResult)
    {
        var output = CraftingSystem.Craft(new[,]
        {
            { ItemStack.Empty, new(materialItemId), ItemStack.Empty },
            { ItemStack.Empty, new(ItemId.Stick), ItemStack.Empty },
            { ItemStack.Empty, new(ItemId.Stick), ItemStack.Empty }
        });

        output.ItemId.Should().Be(expectedResult);
        output.Count.Should().Be(1);
    }

    [DataTestMethod]
    [DataRow(ItemId.PlanksBlock, ItemId.WoodenHoe)]
    [DataRow(ItemId.GoldIngot, ItemId.GoldenHoe)]
    [DataRow(ItemId.CobblestoneBlock, ItemId.StoneHoe)]
    [DataRow(ItemId.IronIngot, ItemId.IronHoe)]
    [DataRow(ItemId.Diamond, ItemId.DiamondHoe)]
    public void CraftingSystem_Craft_Hoes(ItemId materialItemId, ItemId expectedResult)
    {
        var output = CraftingSystem.Craft(new[,]
        {
            { new(materialItemId), new(materialItemId), ItemStack.Empty },
            { ItemStack.Empty, new(ItemId.Stick), ItemStack.Empty },
            { ItemStack.Empty, new(ItemId.Stick), ItemStack.Empty }
        });

        output.ItemId.Should().Be(expectedResult);
        output.Count.Should().Be(1);
    }
}
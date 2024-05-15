using System.Collections.Frozen;
using MineSharp.Items.Infos.Blocks;
using MineSharp.Items.Infos.Items;

namespace MineSharp.Items.Infos;

public static class ItemInfoProvider
{
    private static readonly IReadOnlyDictionary<ItemId, ItemInfo> Data = new Dictionary<ItemId, ItemInfo>
    {
        //Blocks
        { ItemId.StoneBlock, new StoneBlockItemInfo() },
        { ItemId.GrassBlock, new GrassBlockItemInfo() },
        { ItemId.DirtBlock, new DirtBlockItemInfo() },
        { ItemId.CobblestoneBlock, new CobblestoneBlockItemInfo() },
        { ItemId.PlanksBlock, new PlanksBlockItemInfo() },
        { ItemId.SaplingBlock, new SaplingBlockItemInfo() },
        { ItemId.BedrockBlock, new BedrockBlockItemInfo() },
        { ItemId.SpreadingWaterBlock, new SpreadingWaterBlockItemInfo() },
        { ItemId.StillWaterBlock, new StillWaterBlockItemInfo() },
        { ItemId.SpreadingLavaBlock, new SpreadingLavaBlockItemInfo() },
        { ItemId.StillLavaBlock, new StillLavaBlockItemInfo() },
        { ItemId.SandBlock, new SandBlockItemInfo() },
        { ItemId.GravelBlock, new GravelBlockItemInfo() },
        { ItemId.TorchBlock, new TorchBlockItemInfo() },
        { ItemId.LadderBlock, new LadderBlockItemInfo() },

        //Items
        { ItemId.DiamondSword, new DiamondSwordItemInfo() },
        { ItemId.DiamondShovel, new DiamondShovelItemInfo() },
        { ItemId.DiamondPickaxe, new DiamondPickaxeItemInfo() },
        { ItemId.DiamondAxe, new DiamondAxeItemInfo() },
    }.ToFrozenDictionary();

    public static ItemInfo Get(ItemId itemId)
    {
        if (Data.TryGetValue(itemId, out var itemInfo))
            return itemInfo;
        return new PlaceholderItemInfo(itemId); //TODO Replace by exception when ready
    }
}
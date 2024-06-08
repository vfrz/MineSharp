using MineSharp.Sdk.Core;

namespace MineSharp.Content;

public record ToolMaterial
{
    public static readonly ToolMaterial Wood = new(ItemId.PlanksBlock, 0);
    public static readonly ToolMaterial Gold = new(ItemId.GoldIngot, 0);
    public static readonly ToolMaterial Stone = new(ItemId.CobblestoneBlock, 1);
    public static readonly ToolMaterial Iron = new(ItemId.IronIngot, 2);
    public static readonly ToolMaterial Diamond = new(ItemId.Diamond, 3);

    public ItemId ItemId { get; }

    public int Level { get; }

    private ToolMaterial(ItemId itemId, int level)
    {
        ItemId = itemId;
        Level = level;
    }
}
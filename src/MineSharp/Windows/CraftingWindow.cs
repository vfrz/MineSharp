using MineSharp.Content;
using MineSharp.Core;
using MineSharp.Sdk.Core;

namespace MineSharp.Windows;

public class CraftingWindow : Window
{
    public ArrayElement<ItemStack> CraftingOutput { get; }
    public ArraySegment<ItemStack> CraftingGrid { get; }
    public ArraySegment<ItemStack> MainInventory { get; }
    public ArraySegment<ItemStack> Hotbar { get; }

    public CraftingWindow() : base(46)
    {
        CraftingOutput = new ArrayElement<ItemStack>(Slots, 0);
        CraftingGrid = new ArraySegment<ItemStack>(Slots, 1, 9);
        MainInventory = new ArraySegment<ItemStack>(Slots, 10, 27);
        Hotbar = new ArraySegment<ItemStack>(Slots, 37, 9);
    }
}
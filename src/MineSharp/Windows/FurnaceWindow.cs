using MineSharp.Content;
using MineSharp.Core;

namespace MineSharp.Windows;

public class FurnaceWindow : Window
{
    public ArrayElement<ItemStack> Ingredient { get; }
    public ArrayElement<ItemStack> Fuel { get; }
    public ArrayElement<ItemStack> Output { get; }
    public ArraySegment<ItemStack> MainInventory { get; }
    public ArraySegment<ItemStack> Hotbar { get; }

    public FurnaceWindow() : base(39)
    {
        Ingredient = new ArrayElement<ItemStack>(Slots, 0);
        Fuel = new ArrayElement<ItemStack>(Slots, 1);
        Output = new ArrayElement<ItemStack>(Slots, 2);
        MainInventory = new ArraySegment<ItemStack>(Slots, 3, 27);
        Hotbar = new ArraySegment<ItemStack>(Slots, 30, 9);
    }
}
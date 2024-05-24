using MineSharp.Content;

namespace MineSharp.Windows;

public class ChestWindow : Window
{
    public ArraySegment<ItemStack> Chest { get; }
    public ArraySegment<ItemStack> MainInventory { get; }
    public ArraySegment<ItemStack> Hotbar { get; }

    public ChestWindow() : base(63)
    {
        Chest = new ArraySegment<ItemStack>(Slots, 0, 27);
        MainInventory = new ArraySegment<ItemStack>(Slots, 27, 27);
        Hotbar = new ArraySegment<ItemStack>(Slots, 54, 9);
    }
}
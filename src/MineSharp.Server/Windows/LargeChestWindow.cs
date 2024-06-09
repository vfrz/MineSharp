using MineSharp.Content;

namespace MineSharp.Windows;

public class LargeChestWindow : Window
{
    public ArraySegment<ItemStack> Chest { get; }
    public ArraySegment<ItemStack> MainInventory { get; }
    public ArraySegment<ItemStack> Hotbar { get; }

    public LargeChestWindow() : base(90)
    {
        Chest = new ArraySegment<ItemStack>(Slots, 0, 54);
        MainInventory = new ArraySegment<ItemStack>(Slots, 54, 27);
        Hotbar = new ArraySegment<ItemStack>(Slots, 81, 9);
    }
}
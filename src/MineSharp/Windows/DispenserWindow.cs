using MineSharp.Content;
using MineSharp.Sdk.Core;

namespace MineSharp.Windows;

public class DispenserWindow : Window
{
    public ArraySegment<ItemStack> Content { get; }
    public ArraySegment<ItemStack> MainInventory { get; }
    public ArraySegment<ItemStack> Hotbar { get; }

    public DispenserWindow() : base(45)
    {
        Content = new ArraySegment<ItemStack>(Slots, 0, 9);
        MainInventory = new ArraySegment<ItemStack>(Slots, 9, 27);
        Hotbar = new ArraySegment<ItemStack>(Slots, 36, 9);
    }
}
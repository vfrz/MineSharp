using MineSharp.Content;
using MineSharp.Core;
using MineSharp.Extensions;

namespace MineSharp.Windows;

public class Inventory : Window
{
    private const int HotbarOffset = 36;

    public ArrayElement<ItemStack> CraftingOutput { get; }
    public ArraySegment<ItemStack> CraftingGrid { get; }
    public ArraySegment<ItemStack> ArmorGrid { get; }
    public ArraySegment<ItemStack> MainInventory { get; }
    public ArraySegment<ItemStack> Hotbar { get; }

    public Inventory() : base(45)
    {
        CraftingOutput = new ArrayElement<ItemStack>(Slots, 0);
        CraftingGrid = new ArraySegment<ItemStack>(Slots, 0, 5);
        ArmorGrid = new ArraySegment<ItemStack>(Slots, 5, 4);
        MainInventory = new ArraySegment<ItemStack>(Slots, 9, 27);
        Hotbar = new ArraySegment<ItemStack>(Slots, HotbarOffset, 9);
    }

    public short? GetFirstEmptyStorageSlot()
    {
        return Hotbar.FirstEmptyIndex() ?? MainInventory.FirstEmptyIndex();
    }

    public static short HotbarSlotToInventorySlot(short hotbarSlot)
    {
        if (hotbarSlot is < 0 or > 8)
            throw new Exception();
        return (short) (hotbarSlot + HotbarOffset);
    }

    public static byte DataSlotToNetworkSlot(byte index)
    {
        if (index <= 8)
            index += 36;
        else if (index == 100)
            index = 8;
        else if (index == 101)
            index = 7;
        else if (index == 102)
            index = 6;
        else if (index == 103)
            index = 5;
        else if (index is >= 80 and <= 83)
            index -= 79;
        return index;
    }

    public static byte NetworkSlotToDataSlot(byte index)
    {
        if (index is >= 36 and <= 44)
            index -= 36;
        else if (index == 8)
            index = 100;
        else if (index == 7)
            index = 101;
        else if (index == 6)
            index = 102;
        else if (index == 5)
            index = 103;
        else if (index is >= 1 and <= 4)
            index += 79;
        return index;
    }
}
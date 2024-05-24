using MineSharp.Content;

namespace MineSharp.Saves;

public class InventorySlotSaveData
{
    public byte Slot { get; set; }
    public ItemStack ItemStack { get; set; }

    public InventorySlotSaveData(byte slot, ItemStack itemStack)
    {
        Slot = slot;
        ItemStack = itemStack;
    }
}
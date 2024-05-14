namespace MineSharp.Items;

public class Inventory : ItemContainer
{
    private const int HotbarOffset = 36;

    public ArraySegment<ItemStack> CraftingGrid { get; }
    public ArraySegment<ItemStack> ArmorGrid { get; }
    public ArraySegment<ItemStack> MainInventory { get; }
    public ArraySegment<ItemStack> Hotbar { get; }

    public Inventory() : base(45)
    {
        CraftingGrid = new ArraySegment<ItemStack>(Slots, 0, 5);
        ArmorGrid = new ArraySegment<ItemStack>(Slots, 5, 4);
        MainInventory = new ArraySegment<ItemStack>(Slots, 9, 27);
        Hotbar = new ArraySegment<ItemStack>(Slots, HotbarOffset, 9);
    }

    public short? GetFirstEmptyStorageSlot()
    {
        for (var i = 0; i < Hotbar.Count; i++)
        {
            if (Hotbar[i] == ItemStack.Empty)
                return (short) (i + Hotbar.Offset);
        }

        for (var i = 0; i < MainInventory.Count; i++)
        {
            if (MainInventory[i] == ItemStack.Empty)
                return (short) (i + MainInventory.Offset);
        }

        return null;
    }

    public static short HotbarSlotToInventorySlot(short hotbarSlot)
    {
        if (hotbarSlot is < 0 or > 8)
            throw new Exception();
        return (short) (hotbarSlot + HotbarOffset);
    }
}
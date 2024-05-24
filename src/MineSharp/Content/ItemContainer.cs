namespace MineSharp.Content;

public abstract class ItemContainer
{
    public ItemStack[] Slots { get; }

    public ItemContainer(int slots)
    {
        Slots = new ItemStack[slots];
        Clear();
    }

    public void Clear()
    {
        Array.Fill(Slots, ItemStack.Empty);
    }

    public void SetSlot(int index, ItemStack itemStack)
    {
        Slots[index] = itemStack;
    }
}
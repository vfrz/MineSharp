using MineSharp.Content;
using MineSharp.Nbt;
using MineSharp.Nbt.Tags;

namespace MineSharp.TileEntities;

public class ChestTileEntity : TileEntity
{
    public const string Id = "chest";

    public override string EntityId => Id;

    private const int Length = 26;

    //TODO Ensure this list count is alway 26
    public required ItemStack[] Items { get; init; }

    public override INbtTag ToNbt()
    {
        var itemCompounds = new List<INbtTag>();
        for (var i = 0; i < Items.Length; i++)
        {
            var item = Items[i];
            var itemCompound = new CompoundNbtTag(null)
                .AddTag(new ByteNbtTag("Slot", (byte) i))
                .AddTag(new ShortNbtTag("id", (short) item.ItemId))
                .AddTag(new ByteNbtTag("Count", item.Count))
                .AddTag(new ShortNbtTag("Damage", item.Metadata));
            itemCompounds.Add(itemCompound);
        }

        return CreateBaseNbt()
            .AddTag(new ListNbtTag("Items", TagType.Compound, itemCompounds));
    }

    public override void LoadFromNbt(CompoundNbtTag nbtTag)
    {
        base.LoadFromNbt(nbtTag);
        var itemList = nbtTag.Get<ListNbtTag>("Items");
        if (itemList.Tags.Count != Length)
            throw new Exception($"Nbt does not contains exactly {Length} items but: {itemList.Tags.Count}");

        foreach (var itemCompound in itemList.Tags.Cast<CompoundNbtTag>())
        {
            var slot = itemCompound.Get<ByteNbtTag>("Slot").Value;
            var itemId = (ItemId) itemCompound.Get<ShortNbtTag>("id").Value;
            var count = itemCompound.Get<ByteNbtTag>("Count").Value;
            var metadata = itemCompound.Get<ShortNbtTag>("Damage").Value;
            Items[slot] = new ItemStack(itemId, count, metadata);
        }
    }
}
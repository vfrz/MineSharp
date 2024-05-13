using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Items;

namespace MineSharp.Network.Packets;

public class WindowItemsPacket : IServerPacket
{
    public const byte Id = 0x68;
    public byte PacketId => Id;

    public WindowId WindowId { get; set; }
    public IReadOnlyList<ItemStack> Items { get; set; } = [];

    public void Write(PacketWriter writer)
    {
        writer.WriteByte((byte) WindowId);
        writer.WriteShort((short) Items.Count);
        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            writer.WriteShort((short) item.ItemId);
            if (item.ItemId != ItemId.Empty)
            {
                writer.WriteByte(item.Count);
                writer.WriteShort(item.Metadata);
            }
        }
    }
}
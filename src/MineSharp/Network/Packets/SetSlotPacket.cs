using MineSharp.Content;
using MineSharp.Windows;

namespace MineSharp.Network.Packets;

public class SetSlotPacket : IServerPacket
{
    public const int Id = 0x67;
    public byte PacketId => Id;

    public WindowId WindowId { get; set; }
    public short Slot { get; set; }
    public ItemId ItemId { get; set; }
    public byte ItemCount { get; set; }
    public short ItemMetadata { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteByte((byte) WindowId);
        writer.WriteShort(Slot);
        writer.WriteShort((short) ItemId);
        writer.WriteByte(ItemCount);
        if (ItemId != ItemId.Empty)
            writer.WriteShort(ItemMetadata);
    }
}
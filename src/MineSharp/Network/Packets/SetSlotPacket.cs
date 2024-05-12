using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class SetSlotPacket : IServerPacket
{
    public const int Id = 0x67;
    public byte PacketId => Id;

    public byte WindowId { get; set; }
    public short Slot { get; set; }
    public short ItemId { get; set; }
    public byte ItemCount { get; set; }
    public short ItemUses { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteByte(WindowId);
        writer.WriteShort(Slot);
        writer.WriteShort(ItemId);
        writer.WriteByte(ItemCount);
        if (ItemId != -1)
            writer.WriteShort(ItemUses);
    }
}
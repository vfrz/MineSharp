using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class EntityEquipmentPacket : IServerPacket
{
    public const int Id = 0x05;
    public byte PacketId => Id;

    public int EntityId { get; set; }
    public short Slot { get; set; }
    public short ItemId { get; set; }
    public short Unknown { get; set; } //TODO Try to determine

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
        writer.WriteShort(Slot);
        writer.WriteShort(ItemId);
        writer.WriteShort(Unknown);
    }
}
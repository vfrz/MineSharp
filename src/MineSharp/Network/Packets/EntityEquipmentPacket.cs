using MineSharp.Core.Packets;
using MineSharp.Items;

namespace MineSharp.Network.Packets;

public class EntityEquipmentPacket : IServerPacket
{
    public const int Id = 0x05;
    public byte PacketId => Id;

    public int EntityId { get; set; }
    public short Slot { get; set; }
    public ItemId ItemId { get; set; }
    public short Unknown { get; set; } //TODO Try to determine

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
        writer.WriteShort(Slot);
        writer.WriteShort((short) ItemId);
        writer.WriteShort(Unknown);
    }
}
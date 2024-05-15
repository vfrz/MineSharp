using MineSharp.Core.Packets;
using MineSharp.Items;

namespace MineSharp.Network.Packets;

public class PickupSpawnPacket : IServerPacket
{
    public const int Id = 0x15;
    public byte PacketId => Id;

    public int EntityId { get; set; }
    public ItemId ItemId { get; set; }
    public byte Count { get; set; }
    public short Metadata { get; set; }
    public int AbsoluteX { get; set; }
    public int AbsoluteY { get; set; }
    public int AbsoluteZ { get; set; }
    public byte Rotation { get; set; }
    public byte Pitch { get; set; }
    public byte Roll { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
        writer.WriteShort((short)ItemId);
        writer.WriteByte(Count);
        writer.WriteShort(Metadata);
        writer.WriteInt(AbsoluteX);
        writer.WriteInt(AbsoluteY);
        writer.WriteInt(AbsoluteZ);
        writer.WriteByte(Rotation);
        writer.WriteByte(Pitch);
        writer.WriteByte(Roll);
    }
}
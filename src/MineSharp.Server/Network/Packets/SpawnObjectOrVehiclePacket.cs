namespace MineSharp.Network.Packets;

public class SpawnObjectOrVehiclePacket : IServerPacket
{
    public const byte Id = 0x17;
    public byte PacketId => Id;

    public int EntityId { get; set; }
    public byte Type { get; set; }
    public int AbsoluteX { get; set; }
    public int AbsoluteY { get; set; }
    public int AbsoluteZ { get; set; }
    public int UnknownFlag { get; set; }
    public short UnknownValue1 { get; set; }
    public short UnknownValue2 { get; set; }
    public short UnknownValue3 { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
        writer.WriteByte(Type);
        writer.WriteInt(AbsoluteX);
        writer.WriteInt(AbsoluteY);
        writer.WriteInt(AbsoluteZ);
        writer.WriteInt(UnknownFlag);
        if (UnknownFlag > 0)
        {
            writer.WriteShort(UnknownValue1);
            writer.WriteShort(UnknownValue2);
            writer.WriteShort(UnknownValue3);
        }
    }
}
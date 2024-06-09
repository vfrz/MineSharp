namespace MineSharp.Network.Packets;

public class EntityLookPacket : IServerPacket
{
    public const byte Id = 0x20;
    public byte PacketId => Id;

    public int EntityId { get; set; }
    public byte Yaw { get; set; }
    public byte Pitch { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
        writer.WriteByte(Yaw);
        writer.WriteByte(Pitch);
    }
}
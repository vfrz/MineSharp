using MineSharp.Core.Packets;

namespace MineSharp.Packets;

public class NamedEntitySpawnPacket : IServerPacket
{
    public const int Id = 0x14;
    
    public int EntityId { get; set; }
    public string Username { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public sbyte Yaw { get; set; }
    public sbyte Pitch { get; set; }
    public short CurrentItem { get; set; }
    
    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
        writer.WriteInt(EntityId);
        writer.WriteString(Username);
        writer.WriteInt(X);
        writer.WriteInt(Y);
        writer.WriteInt(Z);
        writer.WriteSByte(Yaw);
        writer.WriteSByte(Pitch);
        writer.WriteShort(CurrentItem);
    }
}
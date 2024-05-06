using MineSharp.Core.Packets;

namespace MineSharp.Packets;

public class ThunderboltPacket : IServerPacket
{
    public const int Id = 0x47;
    
    public int EntityId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    
    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
        writer.WriteInt(EntityId);
        writer.WriteBool(true); //Unknown
        writer.WriteInt(X);
        writer.WriteInt(Y);
        writer.WriteInt(Z);
    }
}
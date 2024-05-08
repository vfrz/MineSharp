using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class SpawnPositionPacket : IServerPacket
{
    public const int Id = 0x06;
    
    public int X { get; set; }
    
    public int Y { get; set; }
    
    public int Z { get; set; }
    
    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
        writer.WriteInt(X);
        writer.WriteInt(Y);
        writer.WriteInt(Z);
    }
}
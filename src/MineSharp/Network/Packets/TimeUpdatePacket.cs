using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class TimeUpdatePacket : IServerPacket
{
    public const int Id = 0x04;
    
    public long Time { get; set; }
    
    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
        writer.WriteLong(Time);
    }
}
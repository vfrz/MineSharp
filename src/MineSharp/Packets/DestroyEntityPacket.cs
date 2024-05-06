using MineSharp.Core.Packets;

namespace MineSharp.Packets;

public class DestroyEntityPacket : IServerPacket
{
    public const int Id = 0x1D;
    
    public int EntityId { get; set; }
    
    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
        writer.WriteInt(EntityId);
    }
}
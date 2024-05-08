using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class EntityVelocity : IServerPacket
{
    public const int Id = 0x1C;
    
    public int EntityId { get; set; }
    
    public short VelocityX { get; set; }
    
    public short VelocityY { get; set; }
    
    public short VelocityZ { get; set; }
    
    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
        writer.WriteInt(EntityId);
        writer.WriteShort(VelocityX);
        writer.WriteShort(VelocityY);
        writer.WriteShort(VelocityZ);
    }
}
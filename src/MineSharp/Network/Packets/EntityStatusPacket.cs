using MineSharp.Core.Packets;
using MineSharp.Entities;

namespace MineSharp.Network.Packets;

public class EntityStatusPacket : IServerPacket
{
    public const int Id = 0x26;
    
    public int EntityId { get; set; }
    
    public EntityStatus Status { get; set; }
    
    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
        writer.WriteInt(EntityId);
        writer.WriteByte((byte) Status);
    }
}
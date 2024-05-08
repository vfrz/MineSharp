using MineSharp.Core.Packets;
using MineSharp.Entities.Metadata;

namespace MineSharp.Network.Packets;

public class EntityMetadataPacket : IServerPacket
{
    public const int Id = 0x28;
    
    public int EntityId { get; set; }

    public EntityMetadataContainer Metadata { get; set; } = new();
    
    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
        writer.WriteInt(EntityId);
        Metadata.Write(writer);
    }
}
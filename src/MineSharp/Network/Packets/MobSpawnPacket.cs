using MineSharp.Core.Packets;
using MineSharp.Entities.Metadata;
using MineSharp.Entities.Mobs;

namespace MineSharp.Network.Packets;

public class MobSpawnPacket : IServerPacket
{
    public const int Id = 0x18;

    public byte PacketId => Id;

    public int EntityId { get; set; }
    public MobType Type { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public sbyte Yaw { get; set; }
    public sbyte Pitch { get; set; }
    public EntityMetadataContainer MetadataContainer { get; set; } = new();

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
        writer.WriteByte((byte) Type);
        writer.WriteInt(X);
        writer.WriteInt(Y);
        writer.WriteInt(Z);
        writer.WriteSByte(Yaw);
        writer.WriteSByte(Pitch);
        MetadataContainer.Write(writer);
    }
}
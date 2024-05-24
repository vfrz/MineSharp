using MineSharp.Network;

namespace MineSharp.Entities.Metadata;

public class EntityByteMetadata : IEntityMetadata
{
    public byte Value { get; }

    public EntityByteMetadata(byte value)
    {
        Value = value;
    }

    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Value);
    }
}
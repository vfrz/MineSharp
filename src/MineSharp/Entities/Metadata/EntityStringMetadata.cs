using MineSharp.Core.Packets;

namespace MineSharp.Entities.Metadata;

public class EntityStringMetadata : IEntityMetadata
{
    public string Value { get; }

    public EntityStringMetadata(string value)
    {
        Value = value;
    }

    public void Write(PacketWriter writer)
    {
        writer.WriteString(Value);
    }
}
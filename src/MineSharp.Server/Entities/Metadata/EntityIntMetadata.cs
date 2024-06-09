using MineSharp.Network;

namespace MineSharp.Entities.Metadata;

public class EntityIntMetadata : IEntityMetadata
{
    public int Value { get; }

    public EntityIntMetadata(int value)
    {
        Value = value;
    }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(Value);
    }
}
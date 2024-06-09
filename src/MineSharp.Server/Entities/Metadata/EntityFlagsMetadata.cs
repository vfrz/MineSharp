namespace MineSharp.Entities.Metadata;

public class EntityFlagsMetadata : EntityByteMetadata
{
    public EntityFlags FlagsValue => (EntityFlags) Value;

    public EntityFlagsMetadata(EntityFlags value) : base((byte) value)
    {
    }
}
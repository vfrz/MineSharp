using MineSharp.Core.Packets;

namespace MineSharp.Entities.Metadata;

public class EntityMetadataContainer
{
    private readonly Dictionary<byte, IEntityMetadata> _container = new();

    public void Set(byte index, IEntityMetadata value)
    {
        _container[index] = value;
    }

    public bool TryGet<T>(byte index, out T? metadata) where T : IEntityMetadata
    {
        if (_container.TryGetValue(index, out var value) && value is T valueAsT)
        {
            metadata = valueAsT;
            return true;
        }

        metadata = default;
        return false;
    }

    public void Write(PacketWriter writer)
    {
        foreach (var metadata in _container)
        {
            writer.WriteByte(metadata.Key);
            metadata.Value.Write(writer);
        }

        writer.WriteByte(0x7F);
    }
}
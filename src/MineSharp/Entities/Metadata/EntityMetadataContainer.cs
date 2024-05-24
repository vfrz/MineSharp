using MineSharp.Network;

namespace MineSharp.Entities.Metadata;

public class EntityMetadataContainer
{
    private readonly Dictionary<byte, IEntityMetadata> _container = new();

    public void Set(byte index, IEntityMetadata value)
    {
        _container[index] = value ?? throw new Exception();
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

    public T GetOrDefault<T>(byte index, Func<T> defaultValueProvider) where T : IEntityMetadata
    {
        return TryGet<T>(index, out var metadata) ? metadata! : defaultValueProvider();
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
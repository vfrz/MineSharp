using MineSharp.Network;

namespace MineSharp.Entities.Metadata;

public interface IEntityMetadata
{
    public void Write(PacketWriter writer);
}
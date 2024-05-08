using MineSharp.Core.Packets;

namespace MineSharp.Entities.Metadata;

public interface IEntityMetadata
{
    public void Write(PacketWriter writer);
}
using MineSharp.Core;
using MineSharp.Entities.Metadata;

namespace MineSharp.Entities;

public interface IEntity
{
    public int EntityId { get; }

    public EntityMetadataContainer Metadata { get; }

    public void InitializeEntity(MinecraftServer server, int entityId);
}
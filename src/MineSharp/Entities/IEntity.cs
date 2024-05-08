using MineSharp.Core;

namespace MineSharp.Entities;

public interface IEntity
{
    public int EntityId { get; }

    public void InitializeEntity(MinecraftServer server, int entityId);
}
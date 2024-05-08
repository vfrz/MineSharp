using MineSharp.Core;

namespace MineSharp.Entities;

public abstract class Entity : IEntity
{
    public int EntityId { get; private set; }

    protected MinecraftServer? Server { get; private set; }

    public void InitializeEntity(MinecraftServer server, int entityId)
    {
        if (EntityId != 0)
            throw new Exception("Entity id has already been set");
        EntityId = entityId;
        Server = server;
    }
}
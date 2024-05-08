using System.Collections.Concurrent;
using MineSharp.Core;

namespace MineSharp.Entities;

public class EntityManager
{
    private readonly EntityIdGenerator _idGenerator;
    private readonly ConcurrentDictionary<int, IEntity> _entities;
    private readonly MinecraftServer _server;

    public EntityManager(MinecraftServer server)
    {
        _server = server;
        _idGenerator = new EntityIdGenerator();
        _entities = new ConcurrentDictionary<int, IEntity>();
    }

    public void RegisterEntity(IEntity entity)
    {
        entity.InitializeEntity(_server, _idGenerator.Next());
        _entities.TryAdd(entity.EntityId, entity);
    }

    public void FreeEntity(IEntity entity)
    {
        _entities.Remove(entity.EntityId, out _);
        _idGenerator.Release(entity.EntityId);
    }

    public bool TryGetEntity(int id, out IEntity? entity)
    {
        return _entities.TryGetValue(id, out entity);
    }
}
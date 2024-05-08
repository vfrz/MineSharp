using System.Collections.Concurrent;

namespace MineSharp.Entities;

public class EntityManager
{
    private readonly EntityIdGenerator _idGenerator;
    private readonly ConcurrentDictionary<int, IEntity> _entities;

    public EntityManager()
    {
        _idGenerator = new EntityIdGenerator();
        _entities = new ConcurrentDictionary<int, IEntity>();
    }

    public void RegisterEntity(IEntity entity)
    {
        if (entity.EntityId != 0)
            throw new Exception($"Entity already registered with id: {entity.EntityId}");
        entity.EntityId = _idGenerator.Next();
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
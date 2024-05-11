using System.Collections.Concurrent;
using MineSharp.Core;
using MineSharp.Network.Packets;

namespace MineSharp.Entities;

public class EntityManager
{
    public IEnumerable<IEntity> Entities => _entities.Values;
    
    private readonly ThreadSafeIdGenerator _idGenerator;
    private readonly ConcurrentDictionary<int, IEntity> _entities;
    private readonly MinecraftServer _server;

    private readonly CancellationTokenSource _loopsCts;

    public EntityManager(MinecraftServer server)
    {
        _server = server;

        _idGenerator = new ThreadSafeIdGenerator();
        _entities = new ConcurrentDictionary<int, IEntity>();
        _loopsCts = new CancellationTokenSource();
    }

    public void Start()
    {
        Looper.CreateLoop(TimeSpan.FromSeconds(1), ProcessPickupItemsAsync, _loopsCts.Token);
    }

    public void Stop()
    {
        _loopsCts.Cancel();
    }

    public void RegisterEntity(IEntity entity)
    {
        entity.InitializeEntity(_server, _idGenerator.NextId());
        _entities.TryAdd(entity.EntityId, entity);
    }

    public void FreeEntity(IEntity entity)
    {
        _entities.Remove(entity.EntityId, out _);
    }

    public bool TryGetEntity(int id, out IEntity? entity)
    {
        return _entities.TryGetValue(id, out entity);
    }

    public bool EntityExists(int id) => _entities.ContainsKey(id);
    
    public async Task ProcessPickupItemsAsync()
    {
        foreach (var pickupItem in _entities.Values.OfType<PickupItem>())
        {
            if (pickupItem.IsExpired)
            {
                await _server.BroadcastPacketAsync(new DestroyEntityPacket
                {
                    EntityId = pickupItem.EntityId
                });
                FreeEntity(pickupItem);
            }
        }
    }

    public Task ProcessAsync(TimeSpan elapsed)
    {
        return Task.CompletedTask;
    }
}
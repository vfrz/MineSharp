using MineSharp.Core;
using MineSharp.Entities.Metadata;
using MineSharp.Network.Packets;
using MineSharp.Sdk;

namespace MineSharp.Entities;

public abstract class Entity(MinecraftServer server) : IEntity
{
    public int EntityId { get; private set; } = -1;
    public EntityMetadataContainer Metadata { get; } = new();

    protected MinecraftServer Server { get; } = server;

    public void InitializeEntity(int entityId)
    {
        if (EntityId != -1)
            throw new Exception("Entity has already been initialized");
        EntityId = entityId; 
    }

    protected virtual async Task BroadcastEntityMetadataAsync()
    {
        await Server.BroadcastPacketAsync(new EntityMetadataPacket
        {
            EntityId = EntityId,
            Metadata = Metadata
        });
    }
}
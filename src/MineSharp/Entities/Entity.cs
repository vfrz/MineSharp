using MineSharp.Core;
using MineSharp.Entities.Metadata;
using MineSharp.Network.Packets;

namespace MineSharp.Entities;

public abstract class Entity : IEntity
{
    public int EntityId { get; private set; } = -1;
    public EntityMetadataContainer Metadata { get; } = new();

    protected MinecraftServer? Server { get; private set; }

    public void InitializeEntity(MinecraftServer server, int entityId)
    {
        if (EntityId != -1)
            throw new Exception("Entity has already been initialized");
        EntityId = entityId;
        Server = server;
    }

    protected virtual async Task BroadcastEntityMetadataAsync()
    {
        await Server!.BroadcastPacketAsync(new EntityMetadataPacket
        {
            EntityId = EntityId,
            Metadata = Metadata
        });
    }
}
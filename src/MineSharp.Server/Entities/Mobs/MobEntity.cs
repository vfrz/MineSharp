using MineSharp.Core;
using MineSharp.Entities.Metadata;
using MineSharp.Network.Packets;

namespace MineSharp.Entities.Mobs;

public abstract class MobEntity(MinecraftServer server) : LivingEntity(server), IMobEntity
{
    public abstract MobType Type { get; }

    public EntityMetadataContainer MetadataContainer { get; } = new();

    public override async Task SetHealthAsync(short health)
    {
        Health = Math.Clamp(health, (short) 0, MaxHealth);
        if (Health == 0)
        {
            await Server.BroadcastPacketAsync(new EntityStatusPacket
            {
                EntityId = EntityId,
                Status = EntityStatus.Dead
            });

            Server.Looper.Schedule(TimeSpan.FromSeconds(1.5), async _ =>
            {
                await Server.BroadcastPacketAsync(new DestroyEntityPacket
                {
                    EntityId = EntityId
                });
                Server.EntityManager.FreeEntity(this);
            });
        }
    }

    public async Task BroadcastMetadataAsync()
    {
        await Server.BroadcastPacketAsync(new EntityMetadataPacket
        {
            EntityId = EntityId,
            Metadata = MetadataContainer
        });
    }
}
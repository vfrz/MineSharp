using MineSharp.Entities;
using MineSharp.Network.Packets;
using MineSharp.World;

namespace MineSharp.Core;

public class PlayerEntity : LivingEntity
{
    public const double Height = 1.62;

    public override short MaxHealth => 20;

    public double Stance { get; set; }

    public bool PositionDirty { get; set; }

    public bool Respawning { get; set; }

    private MinecraftRemoteClient RemoteClient { get; }

    public PlayerEntity(MinecraftRemoteClient remoteClient)
    {
        RemoteClient = remoteClient;
    }

    public override async Task SetHealthAsync(short health)
    {
        Health = Math.Clamp(health, (short) 0, MaxHealth);
        await RemoteClient.SendPacketAsync(new UpdateHealthPacket
        {
            Health = health
        });

        if (Health == 0)
        {
            await Server!.BroadcastPacketAsync(new EntityStatusPacket
            {
                EntityId = EntityId,
                Status = EntityStatus.Dead
            }, RemoteClient);

            Server!.Scheduler.Schedule(TimeSpan.FromSeconds(1.5), async () =>
            {
                await Server!.BroadcastPacketAsync(new DestroyEntityPacket
                {
                    EntityId = EntityId
                }, RemoteClient);
            });
        }
    }

    public async Task RespawnAsync(MinecraftDimension dimension)
    {
        Respawning = true;

        var spawnHeight = Server!.World.GetHighestBlockHeight(new Vector2i(0, 0)) + 1.6200000047683716;
        Position = new Vector3d(0.5, spawnHeight, 0.5);
        Stance = Position.Y + Height;
        OnGround = true;
        Yaw = 0;
        Pitch = 0;

        //TODO This is required, but it has an impact of client-side loaded entities, after respawn the player can't interact with other entities
        await RemoteClient.UnloadChunksAsync();

        await RemoteClient.SendPacketAsync(new RespawnPacket
        {
            Dimension = dimension
        });

        await RemoteClient.UpdateLoadedChunksAsync();

        await SendPositionAndLookAsync();

        await RemoteClient.SendPacketAsync(new TimeUpdatePacket
        {
            Time = Server.World.Time
        });

        if (Server.World.Raining)
        {
            await RemoteClient.SendPacketAsync(new NewStatePacket
            {
                Reason = NewStatePacket.ReasonType.BeginRaining
            });
        }

        //TODO Handle spawn point correctly
        await Server!.BroadcastPacketAsync(new NamedEntitySpawnPacket
        {
            EntityId = EntityId,
            X = Position.X.ToAbsoluteInt(),
            Y = Position.Y.ToAbsoluteInt(),
            Z = Position.Z.ToAbsoluteInt(),
            Pitch = MinecraftMath.RotationFloatToSByte(Pitch),
            Yaw = MinecraftMath.RotationFloatToSByte(Yaw),
            Username = RemoteClient.Username!,
            CurrentItem = 0
        }, RemoteClient);

        await SetHealthAsync(MaxHealth);

        Respawning = false;
    }

    private async Task SendPositionAndLookAsync()
    {
        await RemoteClient.SendPacketAsync(new PlayerPositionAndLookServerPacket
        {
            X = Position.X,
            Y = Position.Y,
            Z = Position.Z,
            Stance = Stance,
            OnGround = OnGround,
            Yaw = Yaw,
            Pitch = Pitch
        });
    }
}
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

            //TODO This is a bit dirty
            Server!.Looper.Schedule(TimeSpan.FromSeconds(1.5), async _ =>
            {
                if (Health == 0)
                {
                    await Server!.BroadcastPacketAsync(new DestroyEntityPacket
                    {
                        EntityId = EntityId
                    }, RemoteClient);
                }
            });
        }
    }

    public async Task RespawnAsync(MinecraftDimension dimension)
    {
        //TODO This is a bit dirty
        if (Server!.EntityManager.EntityExists(RemoteClient.Player!.EntityId))
        {
            await Server.BroadcastPacketAsync(new DestroyEntityPacket
            {
                EntityId = RemoteClient.Player!.EntityId
            }, RemoteClient);
        }

        Respawning = true;

        var spawnHeight = Server!.World.GetHighestBlockHeight(new Vector2i(0, 0)) + 1.6200000047683716;
        Position = new Vector3d(0.5, spawnHeight, 0.5);
        Stance = Position.Y + Height;
        OnGround = true;
        Yaw = 0;
        Pitch = 0;

        await RemoteClient.UnloadChunksAsync();

        foreach (var remoteClient in Server.RemoteClients)
        {
            if (remoteClient == RemoteClient || remoteClient.Player is null)
                continue;

            var player = remoteClient.Player!;

            await RemoteClient.SendPacketAsync(new DestroyEntityPacket
            {
                EntityId = player.EntityId
            });
        }

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

        foreach (var remoteClient in Server.RemoteClients)
        {
            if (remoteClient == RemoteClient || remoteClient.Player is null)
                continue;

            var player = remoteClient.Player!;

            await RemoteClient.SendPacketAsync(new NamedEntitySpawnPacket
            {
                EntityId = player.EntityId,
                Username = remoteClient.Username!,
                X = player.Position.X.ToAbsoluteInt(),
                Y = player.Position.Y.ToAbsoluteInt(),
                Z = player.Position.Z.ToAbsoluteInt(),
                Yaw = MinecraftMath.RotationFloatToSByte(Yaw),
                Pitch = MinecraftMath.RotationFloatToSByte(Pitch),
                CurrentItem = 0
            });
        }

        //TODO Handle spawn point correctly
        await Server!.BroadcastPacketAsync(new NamedEntitySpawnPacket
        {
            EntityId = EntityId,
            X = Position.X.ToAbsoluteInt(),
            Y = Position.Y.ToAbsoluteInt(),
            Z = Position.Z.ToAbsoluteInt(),
            Yaw = MinecraftMath.RotationFloatToSByte(Yaw),
            Pitch = MinecraftMath.RotationFloatToSByte(Pitch),
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
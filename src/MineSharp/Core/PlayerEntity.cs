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
        Position = new Vector3d(0, 50, 0);
        Stance = Position.Y + Height;
        OnGround = false;
        Yaw = 0;
        Pitch = 0;
        await RemoteClient.SendPacketAsync(new RespawnPacket
        {
            Dimension = dimension
        });
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
        await SendPositionAndLookAsync(); //This packet surely needs to be send before respawn packet but need TCP batching (channels maybe?)
        await SetHealthAsync(MaxHealth);
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
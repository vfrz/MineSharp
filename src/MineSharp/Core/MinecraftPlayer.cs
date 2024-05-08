using MineSharp.Entities;
using MineSharp.Network.Packets;
using MineSharp.World;

namespace MineSharp.Core;

public class MinecraftPlayer : IEntity
{
    public const double Height = 1.62;

    public int EntityId { get; set; }

    public short Health { get; private set; }

    public const short MaxHealth = 20; 

    public bool OnGround { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public double Z { get; set; }

    public double Stance { get; set; }

    public float Yaw { get; set; }

    public float Pitch { get; set; }

    public bool PositionDirty { get; set; }

    private MinecraftRemoteClient RemoteClient { get; }

    public MinecraftPlayer(MinecraftRemoteClient remoteClient)
    {
        RemoteClient = remoteClient;
    }

    public Task SetHealthAsync(int health) => SetHealthAsync((short) health);
    
    public async Task SetHealthAsync(short health)
    {
        Health = health;
        await RemoteClient.SendPacketAsync(new UpdateHealthPacket
        {
            Health = health
        });
    }

    public async Task KillAsync() => await SetHealthAsync(0);

    public async Task RespawnAsync(MinecraftDimension dimension)
    {
        X = 0;
        Y = 5;
        Z = 0;
        Stance = 5 + Height;
        OnGround = false;
        Yaw = 0;
        Pitch = 0;
        await RemoteClient.SendPacketAsync(new RespawnPacket
        {
            Dimension = dimension
        });
        await SendPositionAndLookAsync(); //This packet surely needs to be send before respawn packet but need TCP batching (channels maybe?)
        await SetHealthAsync(MaxHealth);
    }

    private async Task SendPositionAndLookAsync()
    {
        await RemoteClient.SendPacketAsync(new PlayerPositionAndLookServerPacket
        {
            X = X,
            Y = Y,
            Z = Z,
            Stance = Stance,
            OnGround = OnGround,
            Yaw = Yaw,
            Pitch = Pitch
        });
    }
}
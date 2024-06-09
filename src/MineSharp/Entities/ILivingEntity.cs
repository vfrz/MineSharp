using MineSharp.Numerics;

namespace MineSharp.Entities;

public interface ILivingEntity : IEntity
{
    public Vector3<double> KnockBackMultiplier { get; }

    public Vector3<double> Position { get; }

    public float Yaw { get; }

    public float Pitch { get; }

    public bool OnGround { get; }

    public short Health { get; }

    public short MaxHealth { get; }
    
    public bool IsDead { get; }

    public Task SetHealthAsync(short health);

    public Vector2<int> GetCurrentChunk();
}
using MineSharp.Core;
using MineSharp.Numerics;
using MineSharp.World;

namespace MineSharp.Entities;

public abstract class LivingEntity(MinecraftServer server) : Entity(server), ILivingEntity
{
    public virtual Vector3<double> KnockBackMultiplier => Vector3<double>.One;
    public Vector3<double> Position { get; set; }
    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public bool OnGround { get; set; }
    public short Health { get; protected set; }
    public abstract short MaxHealth { get; }
    public bool IsDead => Health == 0;

    public Vector2<int> GetCurrentChunk() => Chunk.GetChunkPositionForWorldPosition(Position);

    public abstract Task SetHealthAsync(short health);
}
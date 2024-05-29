using MineSharp.Core;

namespace MineSharp.Entities;

public abstract class LivingEntity(MinecraftServer server) : Entity(server), ILivingEntity
{
    public virtual Vector3d KnockBackMultiplier => Vector3d.One;
    public Vector3d Position { get; set; }
    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public bool OnGround { get; set; }
    public short Health { get; protected set; }
    public abstract short MaxHealth { get; }
    public bool IsDead => Health == 0;
    public abstract Task SetHealthAsync(short health);
}
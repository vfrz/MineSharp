using MineSharp.Core;

namespace MineSharp.Entities;

public abstract class LivingEntity : Entity, ILivingEntity
{
    public virtual Vector3 KnockBackMultiplier => Vector3.One;
    public Vector3 Position { get; set; }
    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public bool OnGround { get; set; }

    public short Health { get; protected set; }

    public abstract short MaxHealth { get; }

    public bool Dead => Health == 0;

    public abstract Task SetHealthAsync(short health);
}
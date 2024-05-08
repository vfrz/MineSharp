using MineSharp.Core;

namespace MineSharp.Entities;

public interface ILivingEntity : IEntity
{
    public Vector3 KnockBackMultiplier { get; }

    public Vector3 Position { get; set; }

    public float Yaw { get; set; }

    public float Pitch { get; set; }

    public bool OnGround { get; set; }

    public short Health { get; }

    public short MaxHealth { get; }
    
    public bool Dead { get; }

    public Task SetHealthAsync(short health);
}
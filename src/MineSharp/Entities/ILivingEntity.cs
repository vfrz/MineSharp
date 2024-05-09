using MineSharp.Core;

namespace MineSharp.Entities;

public interface ILivingEntity : IEntity
{
    public Vector3d KnockBackMultiplier { get; }

    public Vector3d Position { get; set; }

    public float Yaw { get; set; }

    public float Pitch { get; set; }

    public bool OnGround { get; set; }

    public short Health { get; }

    public short MaxHealth { get; }
    
    public bool Dead { get; }

    public Task SetHealthAsync(short health);
}
using MineSharp.Entities.Metadata;
using MineSharp.Entities.Mobs;

namespace MineSharp.Entities;

public interface IMobEntity : ILivingEntity
{
    public MobType Type { get; }
    
    public EntityMetadataContainer MetadataContainer { get; }
}
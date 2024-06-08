using MineSharp.Entities.Metadata;
using MineSharp.Entities.Mobs;
using MineSharp.Sdk;

namespace MineSharp.Entities;

public interface IMobEntity : ILivingEntity
{
    public MobType Type { get; }
    
    public EntityMetadataContainer MetadataContainer { get; }
}
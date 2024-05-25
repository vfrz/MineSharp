using MineSharp.Core;

namespace MineSharp.TileEntities;

public abstract class TileEntity
{
    public required string EntityId { get; init; }

    public required Vector3i LocalPosition { get; init; }
}
using MineSharp.Core;
using MineSharp.Nbt.Tags;
using MineSharp.Sdk.Core;

namespace MineSharp.TileEntities;

public abstract class TileEntity
{
    public abstract string EntityId { get; }

    public Vector3<int> LocalPosition { get; private set; }

    public abstract INbtTag ToNbt();

    protected TileEntity()
    {
    }

    protected TileEntity(Vector3<int> localPosition)
    {
        LocalPosition = localPosition;
    }

    public virtual void LoadFromNbt(CompoundNbtTag nbtTag)
    {
        var x = nbtTag.Get<IntNbtTag>("x").Value;
        var y = nbtTag.Get<IntNbtTag>("y").Value;
        var z = nbtTag.Get<IntNbtTag>("z").Value;
        LocalPosition = new Vector3<int>(x, y, z);
    }

    protected CompoundNbtTag CreateBaseNbt()
    {
        return new CompoundNbtTag(null)
            .AddTag(new StringNbtTag("id", EntityId))
            .AddTag(new IntNbtTag("x", LocalPosition.X))
            .AddTag(new IntNbtTag("y", LocalPosition.Y))
            .AddTag(new IntNbtTag("z", LocalPosition.Z));
    }
}
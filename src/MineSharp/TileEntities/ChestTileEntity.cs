using MineSharp.Content;

namespace MineSharp.TileEntities;

public class ChestTileEntity : TileEntity
{
    public required ItemStack[] Items { get; init; }
}
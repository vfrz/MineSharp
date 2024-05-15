using MineSharp.Blocks;
using MineSharp.Core;

namespace MineSharp.World;

public interface IBlockChunkData
{
    public void SetBlock(Vector3i localPosition, BlockId blockId, byte metadata = 0);
    public BlockId GetBlockId(Vector3i localPosition);
    public Block GetBlock(Vector3i localPosition);
    public int GetHighestBlockHeight(Vector2i localPosition);
}
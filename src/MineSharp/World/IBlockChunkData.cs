using MineSharp.Content;
using MineSharp.Core;

namespace MineSharp.World;

public interface IBlockChunkData
{
    public void SetBlock(Vector3<int> localPosition, BlockId blockId, byte metadata = 0);
    public BlockId GetBlockId(Vector3<int> localPosition);
    public Block GetBlock(Vector3<int> localPosition);
    public int GetHighestBlockHeight(Vector2<int> localPosition);
}
using MineSharp.Core;

namespace MineSharp.World;

public interface IBlockChunkData
{
    public void SetBlock(Vector3i localPosition, byte blockId, byte metadata = 0);
    public byte GetBlock(Vector3i localPosition, out byte metadata);

    public byte GetBlock(Vector3i localPosition) => GetBlock(localPosition, out _);
    public int GetHighestBlockHeight(Vector2i localPosition);
}
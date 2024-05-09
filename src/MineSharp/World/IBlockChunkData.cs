using MineSharp.Core;

namespace MineSharp.World;

public interface IBlockChunkData
{
    void SetBlock(Vector3i localPosition, byte blockId, byte metadata = 0);

    byte GetBlock(Vector3i localPosition, out byte metadata);

    byte GetBlock(Vector3i localPosition) => GetBlock(localPosition, out _);
}
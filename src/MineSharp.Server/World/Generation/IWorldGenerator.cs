using MineSharp.Numerics;

namespace MineSharp.World.Generation;

public interface IWorldGenerator
{
    void GenerateChunkTerrain(Vector2<int> chunkPosition, IBlockChunkData chunkData);

    void GenerateChunkDecorations(Vector2<int> chunkPosition, IBlockChunkData chunkData);
}
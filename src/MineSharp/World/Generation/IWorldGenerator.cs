using MineSharp.Core;

namespace MineSharp.World.Generation;

public interface IWorldGenerator
{
    void GenerateChunkTerrain(Vector2i chunkPosition, IBlockChunkData chunkData);

    void GenerateChunkDecorations(Vector2i chunkPosition, IBlockChunkData chunkData);
}
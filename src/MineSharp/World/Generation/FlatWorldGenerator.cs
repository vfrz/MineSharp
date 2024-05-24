using MineSharp.Content;
using MineSharp.Core;

namespace MineSharp.World.Generation;

public class FlatWorldGenerator : IWorldGenerator
{
    private readonly BlockId _blockId;
    private readonly byte _height;

    public FlatWorldGenerator(BlockId blockId = BlockId.Stone, byte height = 42)
    {
        _blockId = blockId;
        _height = height;
    }

    public void GenerateChunkTerrain(Vector2i chunkPosition, IBlockChunkData chunkData)
    {
        for (var x = 0; x < Chunk.ChunkWidth; x++)
        for (var z = 0; z < Chunk.ChunkWidth; z++)
        for (var y = 0; y < _height; y++)
            chunkData.SetBlock(new Vector3i(x, y, z), _blockId);
    }

    public void GenerateChunkDecorations(Vector2i chunkPosition, IBlockChunkData chunkData)
    {
    }
}
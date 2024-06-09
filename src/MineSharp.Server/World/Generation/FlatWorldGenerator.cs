using MineSharp.Content;
using MineSharp.Numerics;

namespace MineSharp.World.Generation;

public class FlatWorldGenerator : IWorldGenerator
{
    private readonly BlockId _blockId;
    private readonly byte _height;

    public FlatWorldGenerator(BlockId blockId = BlockId.Stone, byte height = 64)
    {
        _blockId = blockId;
        _height = height;
    }

    public void GenerateChunkTerrain(Vector2<int> chunkPosition, IBlockChunkData chunkData)
    {
        for (var x = 0; x < Chunk.ChunkWidth; x++)
        for (var z = 0; z < Chunk.ChunkWidth; z++)
        for (var y = 0; y < _height; y++)
            chunkData.SetBlock(new Vector3<int>(x, y, z), _blockId);
    }

    public void GenerateChunkDecorations(Vector2<int> chunkPosition, IBlockChunkData chunkData)
    {
    }
}
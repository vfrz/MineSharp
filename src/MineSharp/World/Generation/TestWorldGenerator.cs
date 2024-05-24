using MineSharp.Content;
using MineSharp.Core;

namespace MineSharp.World.Generation;

public class TestWorldGenerator : IWorldGenerator
{
    public FastNoiseLite Noise { get; }

    public TestWorldGenerator(int seed)
    {
        Noise = new FastNoiseLite(seed);
        Noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        // Terrain parameters
        Noise.SetFrequency(0.8f); // Adjust the frequency to change the terrain detail
        Noise.SetFractalOctaves(16); // Adjust the number of octaves for more complexity
        Noise.SetFractalLacunarity(20.0f); // Adjust the lacunarity for variation
        Noise.SetFractalGain(1f); // Adjust the gain for smoothness
    }

    public void GenerateChunkTerrain(Vector2i chunkPosition, IBlockChunkData chunkData)
    {
        for (var localX = 0; localX < Chunk.ChunkWidth; localX++)
        {
            for (var localZ = 0; localZ < Chunk.ChunkWidth; localZ++)
            {
                // Bedrock
                chunkData.SetBlock(new Vector3i(localX, 0, localZ), BlockId.Bedrock);

                for (var y = 1; y <= 50; y++)
                {
                    chunkData.SetBlock(new Vector3i(localX, y, localZ), BlockId.Stone);
                }
            }
        }
    }

    public void GenerateChunkDecorations(Vector2i chunkPosition, IBlockChunkData chunkData)
    {
        for (var localX = 0; localX < Chunk.ChunkWidth; localX++)
        {
            for (var localZ = 0; localZ < Chunk.ChunkWidth; localZ++)
            {
                var height = (Noise.GetNoise(chunkPosition.X * Chunk.ChunkWidth + localX,
                    chunkPosition.Z * Chunk.ChunkWidth + localZ) + 1) / 2f * 50;

                if (height > 49)
                    chunkData.SetBlock(new Vector3i(localX, 51, localZ), BlockId.Wool, 14);
            }
        }
    }
}
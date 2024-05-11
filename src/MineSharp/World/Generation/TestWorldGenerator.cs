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
        for (var localX = 0; localX < Chunk.Width; localX++)
        {
            for (var localZ = 0; localZ < Chunk.Width; localZ++)
            {
                // Bedrock
                chunkData.SetBlock(new Vector3i(localX, 0, localZ), 7);

                for (var y = 1; y <= 50; y++)
                {
                    chunkData.SetBlock(new Vector3i(localX, y, localZ), 1);
                }
            }
        }
    }

    public void GenerateChunkDecorations(Vector2i chunkPosition, IBlockChunkData chunkData)
    {
        for (var localX = 0; localX < Chunk.Width; localX++)
        {
            for (var localZ = 0; localZ < Chunk.Width; localZ++)
            {
                var height = (Noise.GetNoise(chunkPosition.X * Chunk.Width + localX, chunkPosition.Z * Chunk.Width + localZ) + 1) / 2f * 50;
                
                if (height > 49)
                    chunkData.SetBlock(new Vector3i(localX, 51, localZ), 35, 14);
            }
        }
    }
}
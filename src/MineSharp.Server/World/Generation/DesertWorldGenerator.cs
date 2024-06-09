using MineSharp.Content;
using MineSharp.Numerics;

namespace MineSharp.World.Generation;

public class DesertWorldGenerator : IWorldGenerator
{
    public FastNoiseLite Noise { get; }
    public FastNoiseLite OtherNoise { get; }
    public UniformPoissonDiskSampler PoissonDiskSampler { get; }

    public DesertWorldGenerator(int seed)
    {
        Noise = new FastNoiseLite(seed);
        Noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        // Terrain parameters
        Noise.SetFrequency(0.008f); // Adjust the frequency to change the terrain detail
        Noise.SetFractalOctaves(16); // Adjust the number of octaves for more complexity
        Noise.SetFractalLacunarity(2.0f); // Adjust the lacunarity for variation
        Noise.SetFractalGain(0.5f); // Adjust the gain for smoothness

        OtherNoise = new FastNoiseLite(seed);
        OtherNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        OtherNoise.SetFrequency(0.5f); // Adjust the frequency to change the terrain detail
        OtherNoise.SetFractalOctaves(1); // Adjust the number of octaves for more complexity
        OtherNoise.SetFractalLacunarity(2.0f); // Adjust the lacunarity for variation
        OtherNoise.SetFractalGain(0.5f); // Adjust the gain for smoothness

        PoissonDiskSampler = new UniformPoissonDiskSampler(seed);
    }

    public void GenerateChunkTerrain(Vector2<int> chunkPosition, IBlockChunkData chunkData)
    {
        for (var localX = 0; localX < Chunk.ChunkWidth; localX++)
        {
            for (var localZ = 0; localZ < Chunk.ChunkWidth; localZ++)
            {
                // Bedrock
                chunkData.SetBlock(new Vector3<int>(localX, 0, localZ), BlockId.Bedrock);

                var height = GetHeight(new Vector2<int>(localX, localZ), chunkPosition);
                for (var y = 1; y <= height; y++)
                {
                    if (y == height)
                    {
                        chunkData.SetBlock(new Vector3<int>(localX, y, localZ), BlockId.Sand);

                        var otherNoiseValue = (OtherNoise.GetNoise(chunkPosition.X * Chunk.ChunkWidth + localX,
                            chunkPosition.Z * Chunk.ChunkWidth + localZ) + 1) / 2f;
                        if (otherNoiseValue < 0.2f)
                        {
                            chunkData.SetBlock(new Vector3<int>(localX, y + 1, localZ), BlockId.TallGrass);
                        }
                    }
                    else
                    {
                        chunkData.SetBlock(new Vector3<int>(localX, y, localZ), BlockId.Sand);
                    }
                }
            }
        }
    }

    public void GenerateChunkDecorations(Vector2<int> chunkPosition, IBlockChunkData chunkData)
    {
        var cactuses = PoissonDiskSampler.SampleRectangle(chunkPosition.X * Chunk.ChunkWidth,
            chunkPosition.Z * Chunk.ChunkWidth, Chunk.ChunkWidth, Chunk.ChunkWidth, 6);

        foreach (var cactusPosition in cactuses)
        {
            var localPosition = Chunk.WorldToLocal(Vector2<int>.CreateChecked(cactusPosition));
            var height = GetHeight(localPosition, chunkPosition);

            for (var h = 1; h < 4; h++)
            {
                chunkData.SetBlock(new Vector3<int>(localPosition.X, height + h, localPosition.Z), BlockId.Cactus);
            }
        }
    }

    private int GetHeight(Vector2<int> local, Vector2<int> chunkPosition)
    {
        var noiseValue = (Noise.GetNoise(chunkPosition.X * Chunk.ChunkWidth + local.X,
            chunkPosition.Z * Chunk.ChunkWidth + local.Z) + 1) / 2f;
        return (int)(noiseValue * 30f + 5);
    }
}
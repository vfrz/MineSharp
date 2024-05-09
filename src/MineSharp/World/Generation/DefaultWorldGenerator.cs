using MineSharp.Core;

namespace MineSharp.World.Generation;

public class DefaultWorldGenerator : IWorldGenerator
{
    public FastNoiseLite Noise { get; }
    public FastNoiseLite OtherNoise { get; }
    public UniformPoissonDiskSampler PoissonDiskSampler { get; }

    public DefaultWorldGenerator(int seed)
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
    
    public void GenerateChunk(int chunkX, int chunkZ, IBlockChunkData chunkData)
    {
        for (var localX = 0; localX < Chunk.Width; localX++)
        {
            for (var localZ = 0; localZ < Chunk.Width; localZ++)
            {
                // Bedrock
                chunkData.SetBlock(new Vector3i(localX, 0, localZ), 7);

                var height = GetHeight(localX, localZ, chunkX, chunkZ);
                for (var y = 1; y <= height; y++)
                {
                    if (y == height)
                    {
                        chunkData.SetBlock(new Vector3i(localX, y, localZ), 12);

                        var otherNoiseValue = (OtherNoise.GetNoise(chunkX * Chunk.Width + localX, chunkZ * Chunk.Width + localZ) + 1) / 2f;
                        if (otherNoiseValue < 0.2f)
                        {
                            chunkData.SetBlock(new Vector3i(localX, y + 1, localZ), 31, 0);
                        }
                    }
                    else
                    {
                        chunkData.SetBlock(new Vector3i(localX, y, localZ), 12);
                    }
                }
            }
        }

        var trees = PoissonDiskSampler.SampleRectangle(chunkX * Chunk.Width, chunkZ * Chunk.Width, Chunk.Width, Chunk.Width, 6);

        foreach (var treePosition in trees)
        {
            var localPosition = Chunk.WorldToLocal(new Vector2i(treePosition));
            var height = GetHeight(localPosition.X, localPosition.Z, chunkX, chunkZ);

            for (var h = 1; h < 6; h++)
            {
                chunkData.SetBlock(new Vector3i(localPosition.X, height + h, localPosition.Z), 81);
            }
        }
    }
    
    private int GetHeight(int localX, int localZ, int chunkX, int chunkZ)
    {
        var noiseValue = (Noise.GetNoise(chunkX * Chunk.Width + localX, chunkZ * Chunk.Width + localZ) + 1) / 2f;
        return (int) (noiseValue * 30f + 5);
    }
}
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

    public void GenerateChunkTerrain(Vector2i chunkPosition, IBlockChunkData chunkData)
    {
        for (var localX = 0; localX < Chunk.Width; localX++)
        {
            for (var localZ = 0; localZ < Chunk.Width; localZ++)
            {
                // Bedrock
                chunkData.SetBlock(new Vector3i(localX, 0, localZ), 7);

                var height = GetHeight(new Vector2i(localX, localZ), chunkPosition);
                for (var y = 1; y <= height - 4; y++)
                {
                    chunkData.SetBlock(new Vector3i(localX, y, localZ), 1);
                }

                for (var y = Math.Clamp(height - 4, 1, 255); y <= height; y++)
                {
                    if (y == height)
                    {
                        chunkData.SetBlock(new Vector3i(localX, y, localZ), 2);

                        var otherNoiseValue = (OtherNoise.GetNoise(chunkPosition.X * Chunk.Width + localX, chunkPosition.Z * Chunk.Width + localZ) + 1) / 2f;
                        if (otherNoiseValue < 0.4f)
                        {
                            chunkData.SetBlock(new Vector3i(localX, y + 1, localZ), 31, 1);
                        }
                    }
                    else
                    {
                        chunkData.SetBlock(new Vector3i(localX, y, localZ), 3);
                    }
                }
            }
        }
    }

    public void GenerateChunkDecorations(Vector2i chunkPosition, IBlockChunkData chunkData)
    {
        var trees = PoissonDiskSampler.SampleRectangle(chunkPosition.X * Chunk.Width, chunkPosition.Z * Chunk.Width, Chunk.Width, Chunk.Width, 7);

        foreach (var treePosition in trees)
        {
            var localPosition = Chunk.WorldToLocal(new Vector2i(treePosition));
            var height = GetHeight(localPosition, chunkPosition);
            SpawnTree(new Vector3i(localPosition.X, height + 1, localPosition.Z), chunkData);
        }
    }

    private void SpawnTree(Vector3i position, IBlockChunkData chunkData)
    {
        for (var y = 0; y < 4; y++)
            chunkData.SetBlock(new Vector3i(position.X, position.Y + y, position.Z), 17);

        for (var y = 4; y < 7; y++)
            chunkData.SetBlock(new Vector3i(position.X, position.Y + y, position.Z), 18);

        for (var y = 2; y < 5; y++)
        {
            chunkData.SetBlock(new Vector3i(position.X - 2, position.Y + y, position.Z - 1), 18);
            chunkData.SetBlock(new Vector3i(position.X - 2, position.Y + y, position.Z), 18);
            chunkData.SetBlock(new Vector3i(position.X - 2, position.Y + y, position.Z + 1), 18);

            chunkData.SetBlock(new Vector3i(position.X + 2, position.Y + y, position.Z - 1), 18);
            chunkData.SetBlock(new Vector3i(position.X + 2, position.Y + y, position.Z), 18);
            chunkData.SetBlock(new Vector3i(position.X + 2, position.Y + y, position.Z + 1), 18);

            chunkData.SetBlock(new Vector3i(position.X - 1, position.Y + y, position.Z - 2), 18);
            chunkData.SetBlock(new Vector3i(position.X, position.Y + y, position.Z - 2), 18);
            chunkData.SetBlock(new Vector3i(position.X + 1, position.Y + y, position.Z - 2), 18);

            chunkData.SetBlock(new Vector3i(position.X - 1, position.Y + y, position.Z + 2), 18);
            chunkData.SetBlock(new Vector3i(position.X, position.Y + y, position.Z + 2), 18);
            chunkData.SetBlock(new Vector3i(position.X + 1, position.Y + y, position.Z + 2), 18);
        }

        for (var y = 2; y < 6; y++)
        {
            chunkData.SetBlock(new Vector3i(position.X + 1, position.Y + y, position.Z + 1), 18);
            chunkData.SetBlock(new Vector3i(position.X - 1, position.Y + y, position.Z + 1), 18);
            chunkData.SetBlock(new Vector3i(position.X + 1, position.Y + y, position.Z - 1), 18);
            chunkData.SetBlock(new Vector3i(position.X - 1, position.Y + y, position.Z - 1), 18);
        }

        for (var y = 2; y < 7; y++)
        {
            chunkData.SetBlock(new Vector3i(position.X - 1, position.Y + y, position.Z), 18);
            chunkData.SetBlock(new Vector3i(position.X + 1, position.Y + y, position.Z), 18);
            chunkData.SetBlock(new Vector3i(position.X, position.Y + y, position.Z - 1), 18);
            chunkData.SetBlock(new Vector3i(position.X, position.Y + y, position.Z + 1), 18);
        }
    }

    private int GetHeight(Vector2i local, Vector2i chunkPosition)
    {
        var noiseValue = (Noise.GetNoise(chunkPosition.X * Chunk.Width + local.X, chunkPosition.Z * Chunk.Width + local.Z) + 1) / 2f;
        return (int) (noiseValue * 30f + 5);
    }
}
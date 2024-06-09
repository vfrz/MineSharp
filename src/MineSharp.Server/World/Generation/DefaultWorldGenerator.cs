using MineSharp.Content;
using MineSharp.Numerics;

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

    public void GenerateChunkTerrain(Vector2<int> chunkPosition, IBlockChunkData chunkData)
    {
        for (var localX = 0; localX < Chunk.ChunkWidth; localX++)
        {
            for (var localZ = 0; localZ < Chunk.ChunkWidth; localZ++)
            {
                // Bedrock
                chunkData.SetBlock(new Vector3<int>(localX, 0, localZ), BlockId.Bedrock);

                var height = GetHeight(new Vector2<int>(localX, localZ), chunkPosition);

                for (var y = 1; y <= height - 4; y++)
                {
                    chunkData.SetBlock(new Vector3<int>(localX, y, localZ), BlockId.Stone);
                }

                for (var y = Math.Clamp(height - 4, 1, 255); y <= height; y++)
                {
                    var grass = y == height && y > 62;
                    chunkData.SetBlock(new Vector3<int>(localX, y, localZ), grass ? BlockId.Grass : BlockId.Dirt);
                }

                if (height <= 62)
                {
                    for (var y = height + 1; y < 64; y++)
                    {
                        chunkData.SetBlock(new Vector3<int>(localX, y, localZ), BlockId.StillWater);
                    }
                }
            }
        }
    }

    public void GenerateChunkDecorations(Vector2<int> chunkPosition, IBlockChunkData chunkData)
    {
        var trees = PoissonDiskSampler.SampleRectangle(chunkPosition.X * Chunk.ChunkWidth,
            chunkPosition.Z * Chunk.ChunkWidth, Chunk.ChunkWidth, Chunk.ChunkWidth, 7);

        foreach (var treePosition in trees)
        {
            var localPosition = Chunk.WorldToLocal(Vector2<int>.CreateChecked(treePosition));
            var height = GetHeight(localPosition, chunkPosition);
            if (height > 64)
                SpawnTree(new Vector3<int>(localPosition.X, height + 1, localPosition.Z), chunkData);
        }

        for (var localX = 0; localX < Chunk.ChunkWidth; localX++)
        {
            for (var localZ = 0; localZ < Chunk.ChunkWidth; localZ++)
            {
                var highestBlock = chunkData.GetHighestBlockHeight(new Vector2<int>(localX, localZ));
                var otherNoiseValue = (OtherNoise.GetNoise(chunkPosition.X * Chunk.ChunkWidth + localX,
                    chunkPosition.Z * Chunk.ChunkWidth + localZ) + 1) / 2f;
                if (highestBlock > 62 && otherNoiseValue < 0.4f &&
                    chunkData.GetBlockId(new Vector3<int>(localX, highestBlock, localZ)) == BlockId.Grass)
                {
                    chunkData.SetBlock(new Vector3<int>(localX, highestBlock + 1, localZ), BlockId.TallGrass, 1);
                }
            }
        }
    }

    private void SpawnTree(Vector3<int> position, IBlockChunkData chunkData)
    {
        for (var y = 0; y < 4; y++)
            chunkData.SetBlock(new Vector3<int>(position.X, position.Y + y, position.Z), BlockId.Wood);

        for (var y = 4; y < 7; y++)
            chunkData.SetBlock(new Vector3<int>(position.X, position.Y + y, position.Z), BlockId.Leaves);

        for (var y = 2; y < 5; y++)
        {
            chunkData.SetBlock(new Vector3<int>(position.X - 2, position.Y + y, position.Z - 1), BlockId.Leaves);
            chunkData.SetBlock(new Vector3<int>(position.X - 2, position.Y + y, position.Z), BlockId.Leaves);
            chunkData.SetBlock(new Vector3<int>(position.X - 2, position.Y + y, position.Z + 1), BlockId.Leaves);

            chunkData.SetBlock(new Vector3<int>(position.X + 2, position.Y + y, position.Z - 1), BlockId.Leaves);
            chunkData.SetBlock(new Vector3<int>(position.X + 2, position.Y + y, position.Z), BlockId.Leaves);
            chunkData.SetBlock(new Vector3<int>(position.X + 2, position.Y + y, position.Z + 1), BlockId.Leaves);

            chunkData.SetBlock(new Vector3<int>(position.X - 1, position.Y + y, position.Z - 2), BlockId.Leaves);
            chunkData.SetBlock(new Vector3<int>(position.X, position.Y + y, position.Z - 2), BlockId.Leaves);
            chunkData.SetBlock(new Vector3<int>(position.X + 1, position.Y + y, position.Z - 2), BlockId.Leaves);

            chunkData.SetBlock(new Vector3<int>(position.X - 1, position.Y + y, position.Z + 2), BlockId.Leaves);
            chunkData.SetBlock(new Vector3<int>(position.X, position.Y + y, position.Z + 2), BlockId.Leaves);
            chunkData.SetBlock(new Vector3<int>(position.X + 1, position.Y + y, position.Z + 2), BlockId.Leaves);
        }

        for (var y = 2; y < 6; y++)
        {
            chunkData.SetBlock(new Vector3<int>(position.X + 1, position.Y + y, position.Z + 1), BlockId.Leaves);
            chunkData.SetBlock(new Vector3<int>(position.X - 1, position.Y + y, position.Z + 1), BlockId.Leaves);
            chunkData.SetBlock(new Vector3<int>(position.X + 1, position.Y + y, position.Z - 1), BlockId.Leaves);
            chunkData.SetBlock(new Vector3<int>(position.X - 1, position.Y + y, position.Z - 1), BlockId.Leaves);
        }

        for (var y = 2; y < 7; y++)
        {
            chunkData.SetBlock(new Vector3<int>(position.X - 1, position.Y + y, position.Z), BlockId.Leaves);
            chunkData.SetBlock(new Vector3<int>(position.X + 1, position.Y + y, position.Z), BlockId.Leaves);
            chunkData.SetBlock(new Vector3<int>(position.X, position.Y + y, position.Z - 1), BlockId.Leaves);
            chunkData.SetBlock(new Vector3<int>(position.X, position.Y + y, position.Z + 1), BlockId.Leaves);
        }
    }

    private int GetHeight(Vector2<int> local, Vector2<int> chunkPosition)
    {
        var noiseValue = (Noise.GetNoise(chunkPosition.X * Chunk.ChunkWidth + local.X,
            chunkPosition.Z * Chunk.ChunkWidth + local.Z) + 1) / 2f;
        return (int)(noiseValue * 32) + 54;
    }
}
using System.IO.Compression;
using System.Runtime.CompilerServices;
using MineSharp.Core;

namespace MineSharp.World;

public class WorldChunk
{
    public const int Width = 16;
    public const int Height = 128;
    private const int ArraySize = WorldChunk.Width * WorldChunk.Width * WorldChunk.Height;
    
    private readonly byte[] _blocks;
    private NibbleArray _metadata;
    private NibbleArray _light;
    private NibbleArray _skyLight;

    public MinecraftWorld World { get; }
    public int X { get; }
    public int Z { get; }

    public WorldChunk(MinecraftWorld world, int x, int z)
    {
        World = world;
        X = x;
        Z = z;
        _blocks = new byte[ArraySize + 3 * (ArraySize / 2)];
        _metadata = new NibbleArray(_blocks, ArraySize);
        _light = new NibbleArray(_blocks, ArraySize + ArraySize / 2);
        _skyLight = new NibbleArray(_blocks, ArraySize * 2);
    }

    public void UpdateBlock(int worldX, int worldY, int worldZ, byte blockId, byte metadata = 0)
    {
        var internalX = (worldX % Width + Width) % Width;
        var internalZ = (worldZ % Width + Width) % Width;

        var index = CoordinatesToIndex(internalX, worldY, internalZ);
        _blocks[index] = blockId;
        _metadata[index] = metadata;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CoordinatesToIndex(int chunkX, int chunkY, int chunkZ)
    {
        return chunkY + chunkZ * Height + chunkX * Height * Width;
    }

    public static Vector2i WorldToChunkPosition(Vector3d position)
    {
        var chunkX = (int) position.X / Width - (position.X < 0 ? 1 : 0);
        var chunkZ = (int) position.Z / Width - (position.Z < 0 ? 1 : 0);
        return new Vector2i(chunkX, chunkZ);
    }

    public Vector3i LocalToWorld(Vector3i position) => new(X * Width + position.X, position.Y, Z * Width + position.Z);

    public Vector2i WorldToLocal(Vector2i position)
    {
        var chunkX = position.X / Width;
        var chunkY = position.Z / Width;

        // If the world position is negative, adjust the chunk coordinates accordingly
        if (position.X < 0)
            chunkX -= 1;
        if (position.Z < 0)
            chunkY -= 1;

        // Calculate the local position within the chunk
        var localX = position.X - chunkX * Width;
        var localY = position.Z - chunkY * Width;

        return new(localX, localY);
    }

    public void Generate()
    {
        for (var x = 0; x < Width; x++)
        {
            for (var z = 0; z < Width; z++)
            {
                // Bedrock
                _blocks[CoordinatesToIndex(x, 0, z)] = 7;

                var height = GetHeight(x, z);
                for (var y = 1; y <= height; y++)
                {
                    if (y == height)
                    {
                        _blocks[CoordinatesToIndex(x, y, z)] = 2;

                        var otherNoiseValue = (World.OtherNoise.GetNoise(X * Width + x, Z * Width + z) + 1) / 2f;
                        if (otherNoiseValue < 0.4f)
                        {
                            _blocks[CoordinatesToIndex(x, y + 1, z)] = 31;
                            _metadata[CoordinatesToIndex(x, y + 1, z)] = 0x1;
                        }
                    }
                    else
                    {
                        _blocks[CoordinatesToIndex(x, y, z)] = 3;
                    }
                }

                for (var y = 0; y < Height; y++)
                {
                    var index = CoordinatesToIndex(x, y, z);
                    _light[index] = 15;
                    _skyLight[index] = 15;
                }
            }
        }

        var trees = World.PoissonDiskSampler.SampleRectangle(X * Width, Z * Width, Width, Width, 6)
            .Select(v => WorldToLocal(new Vector2i(v)));

        foreach (var tree in trees)
        {
            var height = GetHeight(tree.X, tree.Z);

            for (var h = 1; h < 6; h++)
            {
                _blocks[CoordinatesToIndex(tree.X, height + h, tree.Z)] = 17;
                _metadata[CoordinatesToIndex(tree.X, height + h, tree.Z)] = 0;
            }
        }
    }

    private int GetHeight(int x, int z)
    {
        var noiseValue = (World.Noise.GetNoise(X * Width + x, Z * Width + z) + 1) / 2f;
        return (int) (noiseValue * 30f + 5);
    }

    public async Task<byte[]> ToCompressedDataAsync()
    {
        var output = new MemoryStream();
        await using (var stream = new ZLibStream(output, CompressionMode.Compress))
        {
            await stream.WriteAsync(_blocks);
        }

        var result = output.ToArray();
        await output.DisposeAsync();
        return result;
    }
}
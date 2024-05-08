using System.IO.Compression;
using System.Runtime.CompilerServices;
using MineSharp.Core;
using MineSharp.Network.Packets;

namespace MineSharp.World;

public class WorldChunk
{
    public const int Length = 16;
    public const int Width = 16;
    public const int Height = 128;
    private const int ArraySize = Length * Width * Height;

    private byte[] _blocks;
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

    public async Task SetBlockAsync(int worldX, int worldY, int worldZ, byte blockId)
    {
        var internalX = (worldX % Length + Length) % Length;
        var internalZ = (worldZ % Width + Width) % Width;

        var index = CoordinatesToIndex(internalX, worldY, internalZ);
        _blocks[index] = blockId;

        var blockUpdatePacket = new BlockUpdatePacket
        {
            X = worldX,
            Y = (sbyte) worldY,
            Z = worldZ,
            BlockId = blockId,
            Metadata = 0
        };

        await World.Server.BroadcastPacketAsync(blockUpdatePacket);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CoordinatesToIndex(int chunkX, int chunkY, int chunkZ)
    {
        return chunkY + chunkZ * Height + chunkX * Height * Width;
    }

    public static Vector2i WorldPositionToChunk(Vector3 position)
    {
        var chunkX = (int) position.X / Length - (position.X < 0 ? 1 : 0);
        var chunkZ = (int) position.Z / Width - (position.Z < 0 ? 1 : 0);
        return new Vector2i(chunkX, chunkZ);
    }

    public void Generate()
    {
        for (var x = 0; x < Length; x++)
        {
            for (var z = 0; z < Width; z++)
            {
                var noiseValue = (World.Noise.GetNoise(X * Length + x, Z * Width + z) + 1) / 2f;

                // Bedrock
                _blocks[CoordinatesToIndex(x, 0, z)] = 7;

                var height = (int) (noiseValue * 30f + 5);
                for (var y = 1; y <= height; y++)
                {
                    if (y == height)
                    {
                        _blocks[CoordinatesToIndex(x, y, z)] = 2;
                        var otherNoiseValue = (World.OtherNoise.GetNoise(X * Length + x, Z * Width + z) + 1) / 2f;
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
using System.IO.Compression;
using System.Runtime.CompilerServices;
using MineSharp.Core;

namespace MineSharp.World;

public class ChunkData
{
    private const int ArraySize = WorldChunk.Width * WorldChunk.Width * WorldChunk.Height;

    private readonly byte[] _blocks;
    private NibbleArray _metadata;
    private NibbleArray _light;
    private NibbleArray _skyLight;

    public ChunkData()
    {
        _blocks = new byte[ArraySize + 3 * (ArraySize / 2)];
        _metadata = new NibbleArray(_blocks, ArraySize);
        _light = new NibbleArray(_blocks, ArraySize + ArraySize / 2);
        _skyLight = new NibbleArray(_blocks, ArraySize * 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LocalToIndex(Vector3i localPosition)
    {
        return localPosition.Y + localPosition.Z * WorldChunk.Height + localPosition.X * WorldChunk.Height * WorldChunk.Width;
    }

    public void SetBlock(Vector3i localPosition, byte blockId, byte metadata = 0)
    {
        var index = LocalToIndex(localPosition);
        _blocks[index] = blockId;
        _metadata[index] = metadata;
    }

    public void SetLight(Vector3i localPosition, byte light, byte skyLight)
    {
        var index = LocalToIndex(localPosition);
        _light[index] = light;
        _skyLight[index] = skyLight;
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
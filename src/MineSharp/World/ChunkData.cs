using System.IO.Compression;
using System.Runtime.CompilerServices;
using MineSharp.Core;

namespace MineSharp.World;

public class ChunkData : IBlockChunkData
{
    private const int ArraySize = Chunk.Width * Chunk.Width * Chunk.Height;

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
        return localPosition.Y + localPosition.Z * Chunk.Height + localPosition.X * Chunk.Height * Chunk.Width;
    }

    public void SetBlock(Vector3i localPosition, byte blockId, byte metadata = 0)
    {
        //TODO Maybe should throw exception when world generation is reworked with multiple phases
        if (localPosition.X is < 0 or >= Chunk.Width 
            || localPosition.Y is < 0 or >= Chunk.Height 
            || localPosition.Z is < 0 or >= Chunk.Width)
            return;
        var index = LocalToIndex(localPosition);
        _blocks[index] = blockId;
        _metadata[index] = metadata;
    }

    public byte GetBlock(Vector3i localPosition, out byte metadata)
    {
        var index = LocalToIndex(localPosition);
        metadata = _metadata[index];
        return _blocks[index];
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
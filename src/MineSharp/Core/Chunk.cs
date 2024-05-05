using System.IO.Compression;

namespace MineSharp.Core;

public class Chunk
{
    public const int Length = 16;
    public const int Width = 16;
    public const int Height = 128;
    private const int ArraySize = Length * Width * Height;

    private byte[] _blocks;
    private NibbleArray _metadata;
    private NibbleArray _light;
    private NibbleArray _skyLight;
    
    public int X { get; }
    public int Z { get; }

    public Chunk(int x, int z)
    {
        X = x;
        Z = z;
        _blocks = new byte[ArraySize + 3 * (ArraySize / 2)];
        _metadata = new NibbleArray(_blocks, ArraySize);
        _light = new NibbleArray(_blocks, ArraySize + ArraySize / 2);
        _skyLight = new NibbleArray(_blocks, ArraySize * 2);
    }

    public void FillDefault()
    {
        for (var x = 0; x < Length; x++)
        {
            for (var z = 0; z < Width; z++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var index = y + z * Height + x * Height * Width;
                    if (y < 10)
                        _blocks[index] = 2;
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
            await stream.WriteAsync(_blocks, 0, _blocks.Length);
        }
        var result = output.ToArray();
        await output.DisposeAsync();
        return result;
    }
}
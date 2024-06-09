using System.IO.Compression;

namespace MineSharp.Extensions;

public static class ByteArrayExtensions
{
    public static byte[] ZLibCompress(this byte[] data)
    {
        using var memoryStream = new MemoryStream();
        using (var zLibStream = new ZLibStream(memoryStream, CompressionMode.Compress))
        {
            zLibStream.Write(data);
        }

        return memoryStream.ToArray();
    }

    public static byte[] ZLibDecompress(this byte[] data)
    {
        using var inputMemoryStream = new MemoryStream(data);
        using var zLibStream = new ZLibStream(inputMemoryStream, CompressionMode.Decompress);
        using var outputMemoryStream = new MemoryStream();
        zLibStream.CopyTo(outputMemoryStream);
        return outputMemoryStream.ToArray();
    }
    
    public static byte[] GZipCompress(this byte[] data)
    {
        using var memoryStream = new MemoryStream();
        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
        {
            gZipStream.Write(data);
        }

        return memoryStream.ToArray();
    }

    public static byte[] GZipDecompress(this byte[] data)
    {
        using var inputMemoryStream = new MemoryStream(data);
        using var gZipStream = new GZipStream(inputMemoryStream, CompressionMode.Decompress);
        using var outputMemoryStream = new MemoryStream();
        gZipStream.CopyTo(outputMemoryStream);
        return outputMemoryStream.ToArray();
    }
}
using System.IO.Compression;
using System.Text;
using MineSharp.Core;

namespace MineSharp.Saves;

public class SaveManager
{
    public void Initialize()
    {
        if (!Directory.Exists("save"))
            Directory.CreateDirectory("save");

        if (!Directory.Exists("save/players"))
            Directory.CreateDirectory("save/players");

        if (!Directory.Exists("save/chunks"))
            Directory.CreateDirectory("save/chunks");
    }

    public async Task SaveChunkAsync(Vector2i chunkPosition, ChunkSaveData chunkSaveData)
    {
        await using var compressedFileStream = File.OpenWrite(GetChunkFilePath(chunkPosition));
        await using var compressor = new DeflateStream(compressedFileStream, CompressionMode.Compress);
        compressor.Write(chunkSaveData.Data);
    }

    public async Task<ChunkSaveData> LoadChunkAsync(Vector2i chunkPosition)
    {
        await using var compressedFileStream = File.OpenRead(GetChunkFilePath(chunkPosition));
        await using var decompressor = new DeflateStream(compressedFileStream, CompressionMode.Decompress);
        using var memoryStream = new MemoryStream();
        await decompressor.CopyToAsync(memoryStream);
        var saveData = new ChunkSaveData
        {
            Data = memoryStream.ToArray()
        };
        return saveData;
    }

    public bool IsChunkSaved(Vector2i chunkPosition) => File.Exists(GetChunkFilePath(chunkPosition));

    public void SaveWorld(WorldSaveData worldSaveData)
    {
        using var compressedFileStream = File.OpenWrite(GetWorldFilePath());
        using var compressor = new DeflateStream(compressedFileStream, CompressionMode.Compress);
        using var writer = new BinaryWriter(compressor);

        writer.Write(worldSaveData.Seed);
        writer.Write(worldSaveData.Time);
        writer.Write(worldSaveData.Raining);
    }

    public WorldSaveData LoadWorld()
    {
        using var compressedFileStream = File.OpenRead(GetWorldFilePath());
        using var decompressor = new DeflateStream(compressedFileStream, CompressionMode.Decompress);
        using var reader = new BinaryReader(decompressor);

        var seed = reader.ReadInt32();
        var time = reader.ReadInt64();
        var raining = reader.ReadBoolean();
        var saveData = new WorldSaveData
        {
            Seed = seed,
            Time = time,
            Raining = raining
        };
        return saveData;
    }

    public bool IsWorldSaved() => File.Exists(GetWorldFilePath());

    private static string GetChunkFilePath(Vector2i chunkPosition)
        => $"save/chunks/x{chunkPosition.X}y{chunkPosition.Z}.dat";

    private static string GetWorldFilePath() => "save/world.dat";
}
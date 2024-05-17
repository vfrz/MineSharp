using System.IO.Compression;
using MineSharp.Core;
using MineSharp.Nbt;
using MineSharp.Nbt.Tags;

namespace MineSharp.Saves;

public class SaveManager
{
    public void Initialize()
    {
        if (!Directory.Exists("world"))
            Directory.CreateDirectory("world");

        if (!Directory.Exists("world/players"))
            Directory.CreateDirectory("world/players");

        if (!Directory.Exists("world/region"))
            Directory.CreateDirectory("world/region");
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
        var dataCompound = new CompoundNbtTag("Data")
            .AddTag(new LongNbtTag("RandomSeed", worldSaveData.Seed))
            .AddTag(new IntNbtTag("SpawnX", worldSaveData.SpawnLocation.X))
            .AddTag(new IntNbtTag("SpawnY", worldSaveData.SpawnLocation.Y))
            .AddTag(new IntNbtTag("SpawnZ", worldSaveData.SpawnLocation.Z))
            .AddTag(new ByteNbtTag("raining", worldSaveData.Raining))
            .AddTag(new IntNbtTag("rainTime", worldSaveData.RainTime))
            .AddTag(new ByteNbtTag("thundering", worldSaveData.Thundering))
            .AddTag(new IntNbtTag("thunderTime", worldSaveData.ThunderTime))
            .AddTag(new LongNbtTag("Time", worldSaveData.Time))
            .AddTag(new IntNbtTag("version", worldSaveData.Version))
            .AddTag(new LongNbtTag("LastPlayed", worldSaveData.LastPlayed))
            .AddTag(new StringNbtTag("LevelName", worldSaveData.LevelName))
            .AddTag(new LongNbtTag("SizeOnDisk", worldSaveData.SizeOnDisk));

        var fileCompound = new CompoundNbtTag(null)
            .AddTag(dataCompound);

        using var fileStream = File.OpenWrite(GetLevelFilePath());
        NbtSerializer.Serialize(fileCompound, fileStream, NbtCompression.Gzip);
    }

    public WorldSaveData LoadWorld()
    {
        using var fileStream = File.OpenRead(GetLevelFilePath());
        var fileCompound = (CompoundNbtTag)NbtSerializer.Deserialize(fileStream, NbtCompression.Gzip);
        var data = fileCompound.Get<CompoundNbtTag>("Data");

        var spawnX = data.Get<IntNbtTag>("SpawnX").Value;
        var spawnY = data.Get<IntNbtTag>("SpawnY").Value;
        var spawnZ = data.Get<IntNbtTag>("SpawnZ").Value;

        var saveData = new WorldSaveData
        {
            Seed = (int)data.Get<LongNbtTag>("RandomSeed").Value,
            SpawnLocation = new Vector3i(spawnX, spawnY, spawnZ),
            Raining = data.Get<ByteNbtTag>("raining").ValueAsBool,
            RainTime = data.Get<IntNbtTag>("rainTime").Value,
            Thundering = data.Get<ByteNbtTag>("thundering").ValueAsBool,
            ThunderTime = data.Get<IntNbtTag>("thunderTime").Value,
            Time = data.Get<LongNbtTag>("Time").Value,
            Version = data.Get<IntNbtTag>("version").Value,
            LastPlayed = data.Get<LongNbtTag>("LastPlayed").Value,
            LevelName = data.Get<StringNbtTag>("LevelName").Value!,
            SizeOnDisk = data.Get<LongNbtTag>("SizeOnDisk").Value
        };
        return saveData;
    }

    public bool IsWorldSaved() => File.Exists(GetLevelFilePath());

    private static string GetChunkFilePath(Vector2i chunkPosition)
        => $"world/region/x{chunkPosition.X}y{chunkPosition.Z}.dat";

    private static string GetLevelFilePath() => "world/level.dat";
}
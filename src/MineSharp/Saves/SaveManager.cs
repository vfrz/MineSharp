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
        await File.WriteAllBytesAsync(GetChunkFilePath(chunkPosition), chunkSaveData.Data);
    }

    public async Task<ChunkSaveData> LoadChunkAsync(Vector2i chunkPosition)
    {
        var data = await File.ReadAllBytesAsync(GetChunkFilePath(chunkPosition));
        var saveData = new ChunkSaveData
        {
            Data = data
        };
        return saveData;
    }

    public bool IsChunkSaved(Vector2i chunkPosition) => File.Exists(GetChunkFilePath(chunkPosition));

    private static string GetChunkFilePath(Vector2i chunkPosition)
        => $"save/chunks/x{chunkPosition.X}z{chunkPosition.Z}.dat";
}
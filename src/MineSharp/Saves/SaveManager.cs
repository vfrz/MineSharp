using MineSharp.Content;
using MineSharp.Core;
using MineSharp.Nbt;
using MineSharp.Nbt.Tags;
using MineSharp.Windows;

namespace MineSharp.Saves;

public static class SaveManager
{
    private const string WorldDirectory = "./world";
    private const string PlayersDirectory = $"{WorldDirectory}/players";
    private const string RegionDirectory = $"{WorldDirectory}/region";
    private const string LevelFile = $"{WorldDirectory}/level.dat";
    private static string GetPlayerSaveFile(string username) => $"{PlayersDirectory}/{username}.dat";
    public static string GetRegionFilePath(Vector2i regionPosition) => $"{RegionDirectory}/r.{regionPosition.X}.{regionPosition.Z}.mcr";

    public static void Initialize()
    {
        CreateDirectoryIfNotExists(WorldDirectory);
        CreateDirectoryIfNotExists(PlayersDirectory);
        CreateDirectoryIfNotExists(RegionDirectory);
    }

    public static bool IsRegionSaved(Vector2i regionPosition) => File.Exists(GetRegionFilePath(regionPosition));

    public static bool IsWorldSaved() => File.Exists(LevelFile);

    public static WorldSaveData LoadWorld()
    {
        using var fileStream = File.OpenRead(LevelFile);
        var fileCompound = (CompoundNbtTag) NbtSerializer.Deserialize(fileStream, NbtCompression.Gzip);
        var data = fileCompound.Get<CompoundNbtTag>("Data");

        var spawnX = data.Get<IntNbtTag>("SpawnX").Value;
        var spawnY = data.Get<IntNbtTag>("SpawnY").Value;
        var spawnZ = data.Get<IntNbtTag>("SpawnZ").Value;

        var saveData = new WorldSaveData
        {
            Seed = (int) data.Get<LongNbtTag>("RandomSeed").Value,
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

    public static void SaveWorld(WorldSaveData worldSaveData)
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

        using var fileStream = File.OpenWrite(LevelFile);
        NbtSerializer.Serialize(fileCompound, fileStream, NbtCompression.Gzip);
    }

    public static bool IsPlayerSaved(string username) => File.Exists(GetPlayerSaveFile(username));

    public static PlayerSaveData LoadPlayer(string username)
    {
        using var fileStream = File.OpenRead(GetPlayerSaveFile(username));
        var playerCompound = (CompoundNbtTag) NbtSerializer.Deserialize(fileStream, NbtCompression.Gzip);

        var positionList = playerCompound.Get<ListNbtTag>("Pos");
        var positionX = positionList.Get<DoubleNbtTag>(0).Value;
        var positionY = positionList.Get<DoubleNbtTag>(1).Value;
        var positionZ = positionList.Get<DoubleNbtTag>(2).Value;

        var rotationList = playerCompound.Get<ListNbtTag>("Rotation");
        var yaw = rotationList.Get<FloatNbtTag>(0).Value;
        var pitch = rotationList.Get<FloatNbtTag>(1).Value;

        var inventoryList = playerCompound.Get<ListNbtTag>("Inventory");
        var inventory = inventoryList.Tags.Cast<CompoundNbtTag>()
            .Select(slotTag =>
            {
                var slot = slotTag.Get<ByteNbtTag>("Slot").Value;
                var itemId = (ItemId) slotTag.Get<ShortNbtTag>("id").Value;
                var count = slotTag.Get<ByteNbtTag>("Count").Value;
                var metadata = slotTag.Get<ShortNbtTag>("Damage").Value;
                return new InventorySlotSaveData(Inventory.DataSlotToNetworkSlot(slot), new ItemStack(itemId, count, metadata));
            }).ToArray();

        var health = playerCompound.Get<ShortNbtTag>("Health").Value;

        var playerSaveData = new PlayerSaveData
        {
            Position = new Vector3d(positionX, positionY, positionZ),
            Yaw = yaw,
            Pitch = pitch,
            Inventory = inventory,
            Health = health
        };

        return playerSaveData;
    }

    public static void SavePlayer(string username, PlayerSaveData playerSaveData)
    {
        var positionList = new List<INbtTag>
        {
            new DoubleNbtTag(null, playerSaveData.Position.X),
            new DoubleNbtTag(null, playerSaveData.Position.Y),
            new DoubleNbtTag(null, playerSaveData.Position.Z)
        };

        var rotationList = new List<INbtTag>
        {
            new FloatNbtTag(null, playerSaveData.Yaw),
            new FloatNbtTag(null, playerSaveData.Pitch)
        };

        var inventoryCompounds = playerSaveData.Inventory
            .Select(slot => new CompoundNbtTag(null)
                .AddTag(new ByteNbtTag("Slot", Inventory.NetworkSlotToDataSlot(slot.Slot)))
                .AddTag(new ShortNbtTag("id", (short) slot.ItemStack.ItemId))
                .AddTag(new ByteNbtTag("Count", slot.ItemStack.Count))
                .AddTag(new ShortNbtTag("Damage", slot.ItemStack.Metadata)))
            .Cast<INbtTag>()
            .ToList();

        var playerCompound = new CompoundNbtTag(null)
            .AddTag(new ListNbtTag("Pos", TagType.Double, positionList))
            .AddTag(new ListNbtTag("Rotation", TagType.Float, rotationList))
            .AddTag(new ListNbtTag("Inventory", TagType.Compound, inventoryCompounds))
            .AddTag(new ShortNbtTag("Health", playerSaveData.Health));

        using var fileStream = File.OpenWrite(GetPlayerSaveFile(username));
        NbtSerializer.Serialize(playerCompound, fileStream, NbtCompression.Gzip);
    }

    private static void CreateDirectoryIfNotExists(string directory)
    {
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }
}
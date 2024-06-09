using MineSharp.Numerics;

namespace MineSharp.Saves;

public struct WorldSaveData
{
    public required int Seed { get; init; }
    public required long Time { get; init; }
    public required Vector3<int> SpawnLocation { get; init; }
    public required bool Raining { get; init; }
    public required int RainTime { get; init; }
    public required bool Thundering { get; init; }
    public required int ThunderTime { get; init; }
    public required int Version { get; init; }
    public required long LastPlayed { get; init; }
    public required string LevelName { get; init; }
    public required long SizeOnDisk { get; init; }
}
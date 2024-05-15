namespace MineSharp.Saves;

public class WorldSaveData
{
    public required int Seed { get; init; }

    public required long Time { get; init; }

    public required bool Raining { get; init; }
}
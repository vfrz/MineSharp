namespace MineSharp.Nbt.Tags;

public readonly struct EndNbtTag : INbtTag
{
    public static readonly EndNbtTag Instance = new();

    public string? Name => null;
}
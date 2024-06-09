namespace MineSharp.World;

public record struct RegionLocation(int Offset, byte Size)
{
    public bool IsEmpty => Offset == 0 && Size == 0;
}
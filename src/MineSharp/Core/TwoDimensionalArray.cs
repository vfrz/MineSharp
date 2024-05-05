using System.Collections;

namespace MineSharp.Core;

public class TwoDimensionalArray<T> : IEnumerable<T>
{
    private readonly T[,] _array;
    public int LowerBoundX { get; }
    public int LowerBoundZ { get; }

    public int UpperBoundX { get; }
    public int UpperBoundZ { get; }

    public TwoDimensionalArray(int lowerBoundX, int upperBoundX, int lowerBoundZ, int upperBoundZ)
    {
        if (lowerBoundX > upperBoundX || lowerBoundZ > upperBoundZ)
            throw new ArgumentException("Lower bound must be less than or equal to upper bound.");

        LowerBoundX = lowerBoundX;
        LowerBoundZ = lowerBoundZ;
        UpperBoundX = upperBoundX;
        UpperBoundZ = upperBoundZ;

        _array = new T[upperBoundX - lowerBoundX + 1, upperBoundZ - lowerBoundZ + 1];
    }

    public T this[int xIndex, int zIndex]
    {
        get
        {
            if (!IsValidIndex(xIndex, zIndex))
                throw new IndexOutOfRangeException("Index out of range.");
            return _array[NormalizeXIndex(xIndex), NormalizeZIndex(zIndex)];
        }
        set
        {
            if (!IsValidIndex(xIndex, zIndex))
                throw new IndexOutOfRangeException("Index out of range.");
            _array[NormalizeXIndex(xIndex), NormalizeZIndex(zIndex)] = value;
        }
    }

    private int NormalizeXIndex(int xIndex)
    {
        return xIndex - LowerBoundX;
    }

    private int NormalizeZIndex(int zIndex)
    {
        return zIndex - LowerBoundZ;
    }

    private bool IsValidIndex(int xIndex, int zIndex)
    {
        return xIndex >= LowerBoundX && xIndex < UpperBoundX && zIndex >= LowerBoundZ && zIndex < UpperBoundZ;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var x = LowerBoundX; x < UpperBoundX; x++)
            for (var z = LowerBoundZ; z < UpperBoundZ; z++)
                yield return this[x, z];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
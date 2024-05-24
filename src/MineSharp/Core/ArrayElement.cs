namespace MineSharp.Core;

public readonly struct ArrayElement<T>(T[] array, int index)
{
    public T Value
    {
        get => array[index];
        set => array[index] = value;
    }
}
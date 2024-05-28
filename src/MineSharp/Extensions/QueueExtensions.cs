namespace MineSharp.Extensions;

public static class QueueExtensions
{
    public static bool IsNotEmpty<T>(this Queue<T> queue) => queue.Count > 0;
    public static bool IsEmpty<T>(this Queue<T> queue) => queue.Count == 0;
}
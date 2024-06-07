namespace MineSharp.Sdk;

public interface ILooper
{
    public void Schedule(TimeSpan delay, Func<CancellationToken, Task> func);

    public void Schedule(TimeSpan delay, Action<CancellationToken> func);
}
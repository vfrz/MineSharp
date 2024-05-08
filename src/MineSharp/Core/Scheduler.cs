using System.Diagnostics;

namespace MineSharp.Core;

//TODO Measure and optimize
public class Scheduler
{
    private readonly Stopwatch _stopwatch = new();
    private readonly PriorityQueue<Func<Task>, long> _entries = new();

    public async Task ProcessAsync()
    {
        if (!_stopwatch.IsRunning)
            return;

        var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
        while (_entries.TryPeek(out var func, out var when) && elapsedMilliseconds >= when)
        {
            try
            {
                await func();
            }
            finally
            {
                _entries.Dequeue();
            }
        }
    }

    public void Start()
    {
        _stopwatch.Start();
    }

    public void Stop()
    {
        _stopwatch.Stop();
    }

    public void Schedule(TimeSpan delay, Func<Task> func)
    {
        _entries.Enqueue(func, _stopwatch.ElapsedMilliseconds + (long) delay.TotalMilliseconds);
    }
}
using System.Diagnostics;

namespace MineSharp.Core;

public static class Looper
{
    public static void CreateLoop(TimeSpan interval, Func<Task> func, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(interval);
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await func();
            }
        }, cancellationToken);
    }
    
    public static void CreateTimedLoop(TimeSpan interval, Func<TimeSpan, Task> func, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            var stopwatch = new Stopwatch();
            using var timer = new PeriodicTimer(interval);
            stopwatch.Start();
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                var elapsed = stopwatch.Elapsed;
                stopwatch.Restart();
                await func(elapsed);
            }
        }, cancellationToken);
    }
}
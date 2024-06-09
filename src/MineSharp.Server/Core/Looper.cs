using System.Collections.Concurrent;
using System.Diagnostics;

namespace MineSharp.Core;

//TODO Measure and optimize if required
public class Looper : ILooper, IDisposable
{
    private record struct TaskToSchedule(Func<CancellationToken, Task> Func, long Delay);

    private class LoopRegistration(Func<CancellationToken, Task> func, long interval, long lastExecution)
    {
        public Func<CancellationToken, Task> Func { get; } = func;

        public long Interval { get; } = interval;

        public long LastExecution { get; set; } = lastExecution;
    }

    private CancellationTokenSource? _cts;
    private Task? _runningTask;

    private readonly List<LoopRegistration> _loops = [];

    private readonly Stopwatch _schedulerStopwatch = new();
    private readonly PriorityQueue<Func<CancellationToken, Task>, long> _scheduledTasks = new();
    private readonly ConcurrentQueue<TaskToSchedule> _tasksToSchedule = new();

    public bool Running { get; private set; }

    public TimeSpan Interval { get; }

    private readonly Func<TimeSpan, CancellationToken, Task> _mainLoop;

    public Looper(TimeSpan interval, Func<TimeSpan, CancellationToken, Task> mainLoop)
    {
        Interval = interval;
        _mainLoop = mainLoop;
    }

    public void Start()
    {
        if (Running)
            return;
        Running = true;
        _cts = new CancellationTokenSource();
        _schedulerStopwatch.Restart();
        _runningTask = Task.Run(ProcessAsync, _cts.Token);
    }

    public async Task StopAsync()
    {
        if (!Running)
            return;
        await _cts!.CancelAsync();
        _schedulerStopwatch.Stop();
        await _runningTask!;
        Running = false;
    }

    public void Schedule(TimeSpan delay, Func<CancellationToken, Task> func)
    {
        var longDelay = _schedulerStopwatch.ElapsedMilliseconds + (long) delay.TotalMilliseconds;
        _tasksToSchedule.Enqueue(new TaskToSchedule(func, longDelay));
    }

    public void Schedule(TimeSpan delay, Action<CancellationToken> func)
    {
        Schedule(delay, token =>
        {
            func(token);
            return Task.CompletedTask;
        });
    }


    public void RegisterLoop(TimeSpan interval, Func<CancellationToken, Task> func, bool executeOnStart = true)
    {
        if (Running)
            throw new Exception("Can't register loop when running");

        if (interval < Interval)
            throw new Exception($"Can't register loop with smaller interval than main loop");

        var longInterval = (long) interval.TotalMilliseconds;
        _loops.Add(new LoopRegistration(func, longInterval, executeOnStart ? -longInterval : 0));
    }

    public void RegisterLoop(TimeSpan interval, Action<CancellationToken> func, bool executeOnStart = true)
    {
        RegisterLoop(interval, token =>
        {
            func(token);
            return Task.CompletedTask;
        }, executeOnStart);
    }

    private async Task ProcessAsync()
    {
        var stopwatch = new Stopwatch();
        using var timer = new PeriodicTimer(Interval);
        stopwatch.Start();
        while (await timer.WaitForNextTickAsync())
        {
            if (_cts!.Token.IsCancellationRequested)
                break;
            var elapsed = stopwatch.Elapsed;
            stopwatch.Restart();
            await ProcessLoopsAsync(elapsed);
            await ProcessScheduledTasksAsync();
        }
    }

    private async Task ProcessLoopsAsync(TimeSpan elapsed)
    {
        await _mainLoop(elapsed, _cts!.Token);
        var now = _schedulerStopwatch.ElapsedMilliseconds;
        foreach (var loop in _loops)
        {
            while (now >= loop.LastExecution + loop.Interval)
            {
                await loop.Func(_cts!.Token);
                loop.LastExecution += loop.Interval;
            }
        }
    }

    private async Task ProcessScheduledTasksAsync()
    {
        if (!_schedulerStopwatch.IsRunning)
            return;

        while (_tasksToSchedule.TryDequeue(out var task))
            _scheduledTasks.Enqueue(task.Func, task.Delay);

        var elapsedMilliseconds = _schedulerStopwatch.ElapsedMilliseconds;
        while (_scheduledTasks.TryPeek(out var func, out var when) && elapsedMilliseconds >= when)
        {
            try
            {
                await func(_cts!.Token);
            }
            finally
            {
                _scheduledTasks.Dequeue();
            }
        }
    }

    public void Dispose()
    {
        _cts?.Dispose();
    }
}
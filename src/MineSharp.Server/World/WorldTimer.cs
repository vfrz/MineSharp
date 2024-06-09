using System.Diagnostics;

namespace MineSharp.World;

public class WorldTimer
{
    private readonly Stopwatch _stopwatch;
    private long _offset;
    
    // 1000ms = 20 minecraft ticks, so we divide real time by 50 (1000/20)
    public long CurrentTime => _stopwatch.ElapsedMilliseconds / 50 + _offset;

    public WorldTimer(long initialTime = 0)
    {
        _stopwatch = new Stopwatch();
        _offset = initialTime;
    }

    public void SetTime(long time)
    {
        _offset = time;
        if (_stopwatch.IsRunning)
            _stopwatch.Restart();
        else
            _stopwatch.Reset();
    }

    public void Start()
    {
        _stopwatch.Start();
    }

    public void Stop()
    {
        _stopwatch.Stop();
    }
}
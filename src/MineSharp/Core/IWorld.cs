using MineSharp.Numerics;

namespace MineSharp.Core;

public interface IWorld
{
    public Task SetTimeAsync(long time);

    public bool Raining { get; }
    public Task StartRainAsync();
    public Task StopRainAsync();

    public Task<int> GetHighestBlockHeightAsync(Vector2<int> worldPosition);
}
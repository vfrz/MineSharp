namespace MineSharp.Sdk;

public interface IServer
{
    public ICommands Commands { get; }

    public IWorld World { get; }

    public ILooper Looper { get; }

    public Task BroadcastChatAsync(string message, IRemoteClient? except = null);

    public void Stop();
}
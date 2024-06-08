namespace MineSharp.Sdk;

public interface IServer
{
    public ICommands Commands { get; }

    public IWorld World { get; }

    public ILooper Looper { get; }

    public IRemoteClient? GetRemoteClientByUsername(string username);
    
    public Task BroadcastChatAsync(string message, IRemoteClient? except = null);

    public Task SaveAsync(CancellationToken cancellationToken);
    public void Stop();
}
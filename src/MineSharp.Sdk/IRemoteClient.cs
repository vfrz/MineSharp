namespace MineSharp.Sdk;

public interface IRemoteClient
{
    public IPlayer? Player { get; }

    public Task SendChatAsync(string message);
}
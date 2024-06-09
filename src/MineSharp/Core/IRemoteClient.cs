namespace MineSharp.Core;

public interface IRemoteClient
{
    public bool Ready { get; }
    public IPlayer? Player { get; }

    public Task SendChatAsync(string message);
    public Task KickAsync(string reason);
}
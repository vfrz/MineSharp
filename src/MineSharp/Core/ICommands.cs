namespace MineSharp.Core;

public interface ICommands
{
    public delegate Task<bool> CommandCallback(IServer server, IRemoteClient? remoteClient, params string[] args);

    public bool TryRegisterCommand(string command, CommandCallback callback);
}
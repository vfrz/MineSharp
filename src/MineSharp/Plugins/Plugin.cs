using MineSharp.Core;

namespace MineSharp.Plugins;

public abstract class Plugin
{
    protected IServer Server { get; }

    protected Plugin(IServer server)
    {
        Server = server;
    }

    public virtual Task OnLoadAsync() => Task.CompletedTask;

    public virtual Task OnServerInitializedAsync() => Task.CompletedTask;

    public virtual Task OnServerStartedAsync() => Task.CompletedTask;

    public virtual Task OnServerStoppingAsync() => Task.CompletedTask;

    public virtual Task OnServerStoppedAsync() => Task.CompletedTask;

    public virtual Task OnPlayerJoiningAsync(IRemoteClient client) => Task.CompletedTask;

    public virtual Task OnPlayerInitializedAsync(IRemoteClient client) => Task.CompletedTask;

    public virtual Task OnPlayerJoinedAsync(IRemoteClient client) => Task.CompletedTask;

    public virtual Task OnTickAsync(TimeSpan elapsed) => Task.CompletedTask;
}
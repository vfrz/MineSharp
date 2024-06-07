namespace MineSharp.Sdk;

public abstract class Plugin
{
    protected IServer Server { get; }

    protected Plugin(IServer server)
    {
        Server = server;
    }

    public virtual Task OnTick(TimeSpan elapsed)
    {
        return Task.CompletedTask;
    }
}
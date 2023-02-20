using Microsoft.Extensions.Hosting;
using MineSharp.Network;

namespace MineSharp.Services;

public class MinecraftServerHostedService : IHostedService
{
    private readonly MinecraftServer _server;

    public MinecraftServerHostedService(MinecraftServer server)
    {
        _server = server;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _server.Stop();
        return Task.CompletedTask;
    }
}
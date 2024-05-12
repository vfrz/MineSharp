using Microsoft.Extensions.Hosting;
using MineSharp.Core;

namespace MineSharp.Services;

public class MinecraftServerHostedService : IHostedService
{
    private readonly MinecraftServer _server;

    public MinecraftServerHostedService(MinecraftServer server)
    {
        _server = server;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _server.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _server.StopAsync();
    }
}
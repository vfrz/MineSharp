using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MineSharp.Core;
using MineSharp.Network;
using MineSharp.Services;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<ServerConfiguration>(context.Configuration);
        
        services.AddMediator();
        
        services.AddSingleton<MinecraftServer>();
        services.AddHostedService<MinecraftServerHostedService>();
    });

await host.RunConsoleAsync();
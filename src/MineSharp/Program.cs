using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MineSharp.Configuration;
using MineSharp.Core;
using MineSharp.Network;
using MineSharp.Services;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
        
        services.Configure<ServerConfiguration>(context.Configuration);
        
        services.AddMediator();
        
        services.AddSingleton<PacketHandler>();
        services.AddSingleton<MinecraftServer>();
        services.AddHostedService<MinecraftServerHostedService>();
    });

await host.RunConsoleAsync();
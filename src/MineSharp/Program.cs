using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MineSharp.Configuration;
using MineSharp.Core;
using MineSharp.Extensions;
using MineSharp.Network;
using MineSharp.Services;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
        
        services.Configure<ServerConfiguration>(context.Configuration);
        
        services.AddSingleton<PacketsHandler>();
        services.AddSingleton<MinecraftServer>();
        services.AddSingleton<EntityIdGenerator>();
        services.AddHostedService<MinecraftServerHostedService>();

        services.RegisterClientPacketHandlers(typeof(Program).Assembly);
    });

await host.RunConsoleAsync();
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MineSharp.Commands;
using MineSharp.Configuration;
using MineSharp.Core;
using MineSharp.Extensions;
using MineSharp.Network;
using MineSharp.Services;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(builder =>
    {
        builder.SetMinimumLevel(LogLevel.Debug);
        builder.AddSimpleConsole(options =>
        {
            options.TimestampFormat = "[HH:mm:ss:fff] ";
        });
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
        
        services.Configure<ServerConfiguration>(context.Configuration);
        
        services.AddSingleton<PacketDispatcher>();
        services.AddSingleton<CommandHandler>();
        services.AddSingleton<MinecraftServer>();
        services.AddHostedService<MinecraftServerHostedService>();

        services.RegisterClientPacketHandlers(typeof(Program).Assembly);
    });

await host.RunConsoleAsync();
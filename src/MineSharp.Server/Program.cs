using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MineSharp.Configuration;
using MineSharp.Core;
using MineSharp.Extensions;
using MineSharp.Network;
using MineSharp.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(builder =>
    {
        builder.AddFilter("Microsoft.", LogLevel.Warning)
            .AddFilter("System.", LogLevel.Warning);
        builder.SetMinimumLevel(LogLevel.Debug);
        builder.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "[HH:mm:ss fff] ";
        });
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);

        services.AddOptions<ServerConfiguration>()
            .Bind(context.Configuration)
            .ValidateDataAnnotations();

        services.AddSingleton<PacketDispatcher>();
        services.AddSingleton<MinecraftServer>();
        services.AddHostedService<MinecraftServerHostedService>();

        services.RegisterClientPacketHandlers(typeof(Program).Assembly);
    });

await host.RunConsoleAsync();
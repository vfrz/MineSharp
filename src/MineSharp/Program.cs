using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MineSharp.Configuration;
using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Network;
using MineSharp.Packets;
using MineSharp.Packets.Handlers;
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
        
        //TODO Automatic packet handlers registration
        services.AddSingleton<IClientPacketHandler<HandshakeRequestPacket>, HandshakeRequestPacketHandler>();
        services.AddSingleton<IClientPacketHandler<ChatMessagePacket>, ChatMessagePacketHandler>();
        services.AddSingleton<IClientPacketHandler<LoginRequestPacket>, LoginRequestPacketHandler>();
        services.AddSingleton<IClientPacketHandler<PlayerDiggingPacket>, PlayerDiggingPacketHandler>();
        services.AddSingleton<IClientPacketHandler<PlayerBlockPlacementPacket>, PlayerBlockPlacementPacketHandler>();
        services.AddSingleton<IClientPacketHandler<PlayerDisconnectPacket>, PlayerDisconnectPacketHandler>();
    });

await host.RunConsoleAsync();
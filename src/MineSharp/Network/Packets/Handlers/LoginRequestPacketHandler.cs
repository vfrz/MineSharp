using Microsoft.Extensions.Logging;
using MineSharp.Core;
using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets.Handlers;

public class LoginRequestPacketHandler : IClientPacketHandler<LoginRequestPacket>
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    private readonly ILogger<MinecraftServer> _logger;

    public LoginRequestPacketHandler(ILogger<MinecraftServer> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(LoginRequestPacket packet, ClientPacketHandlerContext context)
    {
        if (packet.ProtocolVersion != ServerConstants.ProtocolVersion)
        {
            var message = $"{ChatColors.Red}Incompatible Minecraft client, protocol version required: {ServerConstants.ProtocolVersion}";
            await context.RemoteClient.SendPacketAsync(new PlayerDisconnectPacket
            {
                Reason = message
            });
            return;
        }

        var username = packet.Username;

        if (context.Server.Configuration.Debug)
        {
            username = packet.Username + Guid.NewGuid().ToString()[..4];
        }

        await Semaphore.WaitAsync();
        try
        {
            if (context.Server.RemoteClients.Any(c => c.Player?.Username == packet.Username))
            {
                var message = $"{ChatColors.Gold}A user with the same username is already connected, retry later.";
                await context.RemoteClient.SendPacketAsync(new PlayerDisconnectPacket
                {
                    Reason = message
                });
                return;
            }
        }
        finally
        {
            Semaphore.Release();
        }

        var currentPlayer = await context.RemoteClient.InitializePlayerAsync(username);

        await context.RemoteClient.SendPacketAsync(new LoginResponsePacket
        {
            EntityId = context.RemoteClient.Player!.EntityId,
            Dimension = 0,
            MapSeed = context.Server.World.Seed
        });

        context.RemoteClient.SetReady();
        //TODO Everything below is for experimentation and need to be moved somewhere else 

        await currentPlayer.SetHealthAsync(20);

        await currentPlayer.SendInventoryAsync();

        // World chunks and more

        await context.RemoteClient.UpdateLoadedChunksAsync();

        await context.RemoteClient.SendPacketAsync(new TimeUpdatePacket
        {
            Time = context.Server.World.Time
        });

        if (context.Server.World.Raining)
        {
            await context.RemoteClient.SendPacketAsync(new NewStatePacket
            {
                Reason = NewStatePacket.ReasonType.BeginRaining
            });
        }

        //TODO Handle spawn point correctly
        var spawnHeight = await context.Server.World.GetHighestBlockHeightAsync(new Vector2i(0, 0)) + 1;
        await context.RemoteClient.SendPacketAsync(new SpawnPositionPacket
        {
            X = 0,
            Y = spawnHeight,
            Z = 0
        });

        await context.RemoteClient.SendPacketAsync(new PlayerPositionAndLookServerPacket
        {
            X = currentPlayer.Position.X,
            Y = currentPlayer.Position.Y,
            Z = currentPlayer.Position.Z,
            Stance = currentPlayer.Stance,
            Yaw = currentPlayer.Yaw,
            Pitch = currentPlayer.Pitch,
            OnGround = currentPlayer.OnGround
        });

        foreach (var remoteClient in context.Server.RemoteClients)
        {
            if (remoteClient.Player is null
                || remoteClient.Player.Dead
                || remoteClient == context.RemoteClient)
                continue;
            var player = remoteClient.Player!;
            await context.RemoteClient.SendPacketAsync(new NamedEntitySpawnPacket
            {
                EntityId = player.EntityId,
                X = player.Position.X.ToAbsolutePosition(),
                Y = player.Position.Y.ToAbsolutePosition(),
                Z = player.Position.Z.ToAbsolutePosition(),
                Pitch = MinecraftMath.RotationFloatToSByte(player.Pitch),
                Yaw = MinecraftMath.RotationFloatToSByte(player.Yaw),
                Username = player.Username,
                CurrentItem = 0
            });

            await context.RemoteClient.SendPacketAsync(new EntityTeleportPacket
            {
                EntityId = player.EntityId,
                X = player.Position.X.ToAbsolutePosition(),
                Y = player.Position.Y.ToAbsolutePosition(),
                Z = player.Position.Z.ToAbsolutePosition(),
                Pitch = MinecraftMath.RotationFloatToSByte(player.Pitch),
                Yaw = MinecraftMath.RotationFloatToSByte(player.Yaw)
            });
        }

        await context.Server.BroadcastPacketAsync(new NamedEntitySpawnPacket
        {
            EntityId = currentPlayer.EntityId,
            X = currentPlayer.Position.X.ToAbsolutePosition(),
            Y = currentPlayer.Position.Y.ToAbsolutePosition(),
            Z = currentPlayer.Position.Z.ToAbsolutePosition(),
            Pitch = MinecraftMath.RotationFloatToSByte(currentPlayer.Pitch),
            Yaw = MinecraftMath.RotationFloatToSByte(currentPlayer.Yaw),
            Username = currentPlayer.Username,
            CurrentItem = 0
        }, context.RemoteClient);

        await context.Server.BroadcastPacketAsync(new EntityTeleportPacket
        {
            EntityId = currentPlayer.EntityId,
            X = currentPlayer.Position.X.ToAbsolutePosition(),
            Y = currentPlayer.Position.Y.ToAbsolutePosition(),
            Z = currentPlayer.Position.Z.ToAbsolutePosition(),
            Pitch = MinecraftMath.RotationFloatToSByte(currentPlayer.Pitch),
            Yaw = MinecraftMath.RotationFloatToSByte(currentPlayer.Yaw)
        }, context.RemoteClient);

        _logger.LogInformation($"Player {currentPlayer.Username} ({context.RemoteClient.NetworkId}) has joined the server");

        var welcomeMessage = $"Welcome on MineSharp, {ChatColors.Blue}{currentPlayer.Username}{ChatColors.White}!";
        await context.RemoteClient.SendChatAsync(welcomeMessage);

        var joinedMessage = $"{ChatColors.Blue}{currentPlayer.Username} {ChatColors.White}has joined the server!";
        await context.Server.BroadcastChatAsync(joinedMessage, context.RemoteClient);
    }
}
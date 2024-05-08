using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.World;

namespace MineSharp.Network.Packets.Handlers;

public class LoginRequestPacketHandler : IClientPacketHandler<LoginRequestPacket>
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

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

        if (context.Server.Configuration.Debug)
        {
            context.RemoteClient.Username = packet.Username + Guid.NewGuid().ToString()[..4];
        }
        else
        {
            await Semaphore.WaitAsync();
            try
            {
                if (context.Server.RemoteClients.Any(c => c.Username == packet.Username))
                {
                    var message = $"{ChatColors.Gold}A user with the same username is already connected, retry later.";
                    await context.RemoteClient.SendPacketAsync(new PlayerDisconnectPacket
                    {
                        Reason = message
                    });
                    return;
                }

                context.RemoteClient.Username = packet.Username;
            }
            finally
            {
                Semaphore.Release();
            }
        }

        var currentPlayer = context.RemoteClient.InitializePlayer();

        await context.RemoteClient.SendPacketAsync(new LoginResponsePacket
        {
            EntityId = context.RemoteClient.Player!.EntityId,
            Dimension = 0,
            MapSeed = 0
        });

        context.RemoteClient.SetReady();
        //TODO Everything below is for experimentation and need to be moved somewhere else 

        await currentPlayer.SetHealthAsync(20);

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

        await context.RemoteClient.SendPacketAsync(new SpawnPositionPacket
        {
            X = 0,
            Y = 5,
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

        // Might have a bug around here, sometime the client crashes on joining
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
                X = player.Position.X.ToAbsoluteInt(),
                Y = player.Position.Y.ToAbsoluteInt(),
                Z = player.Position.Z.ToAbsoluteInt(),
                Pitch = MinecraftMath.RotationFloatToSByte(player.Pitch),
                Yaw = MinecraftMath.RotationFloatToSByte(player.Yaw),
                Username = remoteClient.Username!,
                CurrentItem = 0
            });

            await context.RemoteClient.SendPacketAsync(new EntityTeleportPacket
            {
                EntityId = player.EntityId,
                X = player.Position.X.ToAbsoluteInt(),
                Y = player.Position.Y.ToAbsoluteInt(),
                Z = player.Position.Z.ToAbsoluteInt(),
                Pitch = MinecraftMath.RotationFloatToSByte(player.Pitch),
                Yaw = MinecraftMath.RotationFloatToSByte(player.Yaw)
            });
        }

        await context.Server.BroadcastPacketAsync(new NamedEntitySpawnPacket
        {
            EntityId = currentPlayer.EntityId,
            X = currentPlayer.Position.X.ToAbsoluteInt(),
            Y = currentPlayer.Position.Y.ToAbsoluteInt(),
            Z = currentPlayer.Position.Z.ToAbsoluteInt(),
            Pitch = MinecraftMath.RotationFloatToSByte(currentPlayer.Pitch),
            Yaw = MinecraftMath.RotationFloatToSByte(currentPlayer.Yaw),
            Username = context.RemoteClient.Username!,
            CurrentItem = 0
        }, context.RemoteClient);

        await context.Server.BroadcastPacketAsync(new EntityTeleportPacket
        {
            EntityId = currentPlayer.EntityId,
            X = currentPlayer.Position.X.ToAbsoluteInt(),
            Y = currentPlayer.Position.Y.ToAbsoluteInt(),
            Z = currentPlayer.Position.Z.ToAbsoluteInt(),
            Pitch = MinecraftMath.RotationFloatToSByte(currentPlayer.Pitch),
            Yaw = MinecraftMath.RotationFloatToSByte(currentPlayer.Yaw)
        }, context.RemoteClient);


        var welcomeMessage = $"Welcome on MineSharp, {ChatColors.Blue}{context.RemoteClient.Username}{ChatColors.White}!";
        await context.RemoteClient.SendMessageAsync(welcomeMessage);

        var joinedMessage = $"{ChatColors.Blue}{context.RemoteClient.Username} {ChatColors.White}has joined the server!";
        await context.Server.BroadcastMessageAsync(joinedMessage, context.RemoteClient);

        await context.RemoteClient.SendPacketAsync(new SetSlotPacket
        {
            WindowId = 0,
            Slot = 36,
            ItemId = 3,
            ItemCount = 64,
            ItemUses = 0
        });

        await context.RemoteClient.SendPacketAsync(new SetSlotPacket
        {
            WindowId = 0,
            Slot = 37,
            ItemId = 277,
            ItemCount = 1,
            ItemUses = 0
        });

        await context.RemoteClient.SendPacketAsync(new SetSlotPacket
        {
            WindowId = 0,
            Slot = 38,
            ItemId = 1,
            ItemCount = 80,
            ItemUses = 0
        });
        
        await context.RemoteClient.SendPacketAsync(new SetSlotPacket
        {
            WindowId = 0,
            Slot = 39,
            ItemId = 345,
            ItemCount = 1,
            ItemUses = 0
        });
    }
}
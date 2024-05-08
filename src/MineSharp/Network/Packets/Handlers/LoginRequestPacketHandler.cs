using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.World;

namespace MineSharp.Network.Packets.Handlers;

public class LoginRequestPacketHandler : IClientPacketHandler<LoginRequestPacket>
{
    public async Task HandleAsync(LoginRequestPacket packet, ClientPacketHandlerContext context)
    {
        context.RemoteClient.Username = packet.Username + Guid.NewGuid().ToString()[..4];

        if (packet.ProtocolVersion != ServerConstants.ProtocolVersion)
        {
            var message = $"{ChatColors.Red}Incompatible Minecraft client, protocol version required: {ServerConstants.ProtocolVersion}";
            await context.RemoteClient.SendPacketAsync(new PlayerDisconnectPacket
            {
                Reason = message
            });
        }
        else
        {
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
            foreach (var chunk in context.Server.World.Chunks)
            {
                if (chunk is null)
                    continue;

                await context.RemoteClient.SendPacketAsync(new PreChunkPacket
                {
                    X = chunk.X,
                    Z = chunk.Z,
                    Mode = PreChunkPacket.LoadingMode.Load
                });

                await context.RemoteClient.SendPacketAsync(new ChunkPacket
                {
                    X = chunk.X * WorldChunk.Length,
                    Y = 0,
                    Z = chunk.Z * WorldChunk.Width,
                    SizeX = WorldChunk.Length - 1,
                    SizeY = WorldChunk.Height - 1,
                    SizeZ = WorldChunk.Width - 1,
                    CompressedData = await chunk.ToCompressedDataAsync()
                });
            }

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
                X = currentPlayer.X,
                Stance = currentPlayer.Stance,
                Y = currentPlayer.Y,
                Z = currentPlayer.Z,
                Yaw = currentPlayer.Yaw,
                Pitch = currentPlayer.Pitch,
                OnGround = currentPlayer.OnGround
            });

            // Might have a bug around here, sometime the client crashes on joining
            foreach (var remoteClient in context.Server.RemoteClients)
            {
                if (remoteClient.Player is null || remoteClient == context.RemoteClient)
                    continue;
                var player = remoteClient.Player!;
                await context.RemoteClient.SendPacketAsync(new NamedEntitySpawnPacket
                {
                    EntityId = player.EntityId,
                    X = (int) (player.X * 32),
                    Z = (int) (player.Z * 32),
                    Y = (int) (player.Y * 32),
                    Pitch = MinecraftMath.RotationFloatToSByte(player.Pitch),
                    Yaw = MinecraftMath.RotationFloatToSByte(player.Yaw),
                    Username = remoteClient.Username!,
                    CurrentItem = 0
                });

                await context.RemoteClient.SendPacketAsync(new EntityTeleportPacket
                {
                    EntityId = player.EntityId,
                    X = (int) (player.X * 32),
                    Z = (int) (player.Z * 32),
                    Y = (int) (player.Y * 32),
                    Pitch = MinecraftMath.RotationFloatToSByte(player.Pitch),
                    Yaw = MinecraftMath.RotationFloatToSByte(player.Yaw)
                });
            }

            await context.Server.BroadcastPacketAsync(new NamedEntitySpawnPacket
            {
                EntityId = currentPlayer.EntityId,
                X = (int) (currentPlayer.X * 32),
                Z = (int) (currentPlayer.Z * 32),
                Y = (int) (currentPlayer.Y * 32),
                Pitch = MinecraftMath.RotationFloatToSByte(currentPlayer.Pitch),
                Yaw = MinecraftMath.RotationFloatToSByte(currentPlayer.Yaw),
                Username = context.RemoteClient.Username!,
                CurrentItem = 0
            }, context.RemoteClient);

            await context.Server.BroadcastPacketAsync(new EntityTeleportPacket
            {
                EntityId = currentPlayer.EntityId,
                X = (int) (currentPlayer.X * 32),
                Z = (int) (currentPlayer.Z * 32),
                Y = (int) (currentPlayer.Y * 32),
                Pitch = MinecraftMath.RotationFloatToSByte(currentPlayer.Pitch),
                Yaw = MinecraftMath.RotationFloatToSByte(currentPlayer.Yaw)
            }, context.RemoteClient);

            await context.Server.BroadcastMessageAsync($"{ChatColors.Blue}{context.RemoteClient.Username} {ChatColors.White}has joined the server!");

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
        }
    }
}
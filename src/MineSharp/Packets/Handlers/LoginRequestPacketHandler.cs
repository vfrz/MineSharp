using MineSharp.Core;
using MineSharp.Core.Packets;

namespace MineSharp.Packets.Handlers;

public class LoginRequestPacketHandler : IClientPacketHandler<LoginRequestPacket>
{
    private readonly EntityIdGenerator _entityIdGenerator;

    public LoginRequestPacketHandler(EntityIdGenerator entityIdGenerator)
    {
        _entityIdGenerator = entityIdGenerator;
    }

    public async Task HandleAsync(LoginRequestPacket packet, ClientPacketHandlerContext context)
    {
        context.RemoteClient.Username = packet.Username + Guid.NewGuid().ToString()[..4];

        if (packet.ProtocolVersion != ServerConstants.ProtocolVersion)
        {
            var message = $"Incompatible Minecraft client, protocol version required: {ServerConstants.ProtocolVersion}";
            using var session = context.RemoteClient.SocketWrapper.StartWriting();
            session.WriteByte(0xFF);
            await session.WriteStringAsync(message);
        }
        else
        {
            using (var session = context.RemoteClient.SocketWrapper.StartWriting())
            {
                session.WriteByte(0x01);
                // Player entity id
                await session.WriteIntAsync(1);
                // Not used, may be the server name
                await session.WriteStringAsync("");
                // Map seed, not used by the client
                await session.WriteLongAsync(0);
                // Dimension
                session.WriteByte(0);
            }

            context.RemoteClient.InitializePlayer(new MinecraftPlayer
            {
                EntityId = _entityIdGenerator.Next(),
                Health = 20
            });

            //TODO Everything below is for experimentation and need to be moved somewhere else 

            // World chunks
            foreach (var chunk in context.Server.World.Chunks)
            {
                if (chunk is null)
                    continue;

                using (var session = context.RemoteClient.SocketWrapper.StartWriting())
                {
                    session.WriteByte(0x32);
                    await session.WriteIntAsync(chunk.X);
                    await session.WriteIntAsync(chunk.Z);
                    session.WriteByte(1);
                }

                using (var session = context.RemoteClient.SocketWrapper.StartWriting())
                {
                    session.WriteByte(0x33);
                    await session.WriteIntAsync(chunk.X * Chunk.Length);
                    await session.WriteUInt16Async(0);
                    await session.WriteIntAsync(chunk.Z * Chunk.Width);
                    session.WriteByte(Chunk.Length - 1);
                    session.WriteByte(Chunk.Height - 1);
                    session.WriteByte(Chunk.Width - 1);

                    var compressedData = await chunk.ToCompressedDataAsync();
                    await session.WriteIntAsync(compressedData.Length);
                    await session.WriteBytesAsync(compressedData);
                }
            }

            // Spawn point
            using (var session = context.RemoteClient.SocketWrapper.StartWriting())
            {
                session.WriteByte(0x06);
                await session.WriteIntAsync(0);
                await session.WriteIntAsync(0);
                await session.WriteIntAsync(20);
            }

            // Set position
            using (var session = context.RemoteClient.SocketWrapper.StartWriting())
            {
                session.WriteByte(0x0D);
                await session.WriteDoubleAsync(0);
                await session.WriteDoubleAsync(20 + 1.62);
                await session.WriteDoubleAsync(20);
                await session.WriteDoubleAsync(0);
                await session.WriteFloatAsync(0);
                await session.WriteFloatAsync(0);
                session.WriteByte(1);
            }

            await context.Server.BroadcastMessageAsync($"{ChatColors.Blue}{context.RemoteClient.Username} {ChatColors.White}has joined the server!");

            using (var session = context.RemoteClient.SocketWrapper.StartWriting())
            {
                session.WriteByte(0x67);
                session.WriteByte(0);
                await session.WriteUInt16Async(36);
                await session.WriteUInt16Async(3);
                session.WriteByte(64);
                await session.WriteUInt16Async(0);
            }

            using (var session = context.RemoteClient.SocketWrapper.StartWriting())
            {
                session.WriteByte(0x67);
                session.WriteByte(0);
                await session.WriteUInt16Async(37);
                await session.WriteUInt16Async(277);
                session.WriteByte(1);
                await session.WriteUInt16Async(0);
            }

            Task.Run(async () =>
            {
                var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

                while (await timer.WaitForNextTickAsync())
                {
                    // Keep alive
                    using (var session = context.RemoteClient.SocketWrapper.StartWriting())
                    {
                        session.WriteByte(0x0);
                    }

                    /*if (packet.Client.Player!.Health > 0)
                    {
                        packet.Client.Player!.Health -= 2;

                        using (var session = packet.Client.SocketWrapper.StartWriting())
                        {
                            session.WriteByte(0x26);
                            await session.WriteIntAsync(1);
                            session.WriteByte(2);
                        }

                        using (var session = packet.Client.SocketWrapper.StartWriting())
                        {
                            session.WriteByte(0x08);
                            await session.WriteShortAsync(packet.Client.Player!.Health);
                        }
                    }*/
                }
            });
        }
    }
}
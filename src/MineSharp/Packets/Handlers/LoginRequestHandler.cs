using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Network;

namespace MineSharp.Packets.Handlers;

public class LoginRequestHandler : IPacketHandler<LoginRequest>
{
    private readonly MinecraftServer _server;

    public LoginRequestHandler(MinecraftServer server)
    {
        _server = server;
    }

    public async ValueTask HandleAsync(LoginRequest command, CancellationToken cancellationToken)
    {
        command.Client.Username = command.Username + Guid.NewGuid().ToString()[..4];

        if (command.ProtocolVersion != ServerConstants.ProtocolVersion)
        {
            var message = $"Incompatible Minecraft client, protocol version required: {ServerConstants.ProtocolVersion}";
            using var session = command.Client.SocketWrapper.StartWriting();
            session.WriteByte(0xFF);
            await session.WriteStringAsync(message);
        }
        else
        {
            using (var session = command.Client.SocketWrapper.StartWriting())
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

            command.Client.InitializePlayer();

            //TODO Everything below is for experimentation and need to be moved somewhere else 
            
            // World chunks
            foreach (var chunk in _server.World.Chunks)
            {
                if (chunk is null)
                    continue;

                using (var session = command.Client.SocketWrapper.StartWriting())
                {
                    session.WriteByte(0x32);
                    await session.WriteIntAsync(chunk.X);
                    await session.WriteIntAsync(chunk.Z);
                    session.WriteByte(1);
                }

                using (var session = command.Client.SocketWrapper.StartWriting())
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
            using (var session = command.Client.SocketWrapper.StartWriting())
            {
                session.WriteByte(0x06);
                await session.WriteIntAsync(0);
                await session.WriteIntAsync(0);
                await session.WriteIntAsync(20);
            }

            // Set position
            using (var session = command.Client.SocketWrapper.StartWriting())
            {
                session.WriteByte(0x0D);
                await session.WriteDoubleAsync(0);
                await session.WriteDoubleAsync(20 + 1.62);
                await session.WriteDoubleAsync(20);
                await session.WriteDoubleAsync(0);
                await session.WriteFloatAsync(0);
                await session.WriteFloatAsync(0);
                session.WriteByte(0);
            }

            await _server.BroadcastMessageAsync($"§9{command.Client.Username} §fhas joined the server!");

            using (var session = command.Client.SocketWrapper.StartWriting())
            {
                session.WriteByte(0x67);
                session.WriteByte(0);
                await session.WriteUInt16Async(36);
                await session.WriteUInt16Async(3);
                session.WriteByte(64);
                await session.WriteUInt16Async(0);
            }

            Task.Run(async () =>
            {
                var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

                while (await timer.WaitForNextTickAsync())
                {
                    // Keep alive
                    using (var session = command.Client.SocketWrapper.StartWriting())
                    {
                        session.WriteByte(0x0);
                    }

                    /*if (command.Client.Player!.Health > 0)
                    {
                        command.Client.Player!.Health -= 2;

                        using (var session = command.Client.SocketWrapper.StartWriting())
                        {
                            session.WriteByte(0x26);
                            await session.WriteIntAsync(1);
                            session.WriteByte(2);
                        }

                        using (var session = command.Client.SocketWrapper.StartWriting())
                        {
                            session.WriteByte(0x08);
                            await session.WriteShortAsync(command.Client.Player!.Health);
                        }
                    }*/
                }
            });
        }
    }
}
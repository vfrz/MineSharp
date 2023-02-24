using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Packets.Handlers;

public class LoginStartHandler : IPacketHandler<LoginStart>
{
    public async ValueTask HandleAsync(LoginStart command, CancellationToken cancellationToken)
    {
        command.Client.Username = command.Name;
        command.Client.Id = command.PlayerUniqueId ?? throw new Exception("Player has no id");
        
        if (command.Client.ProtocolVersion != ServerConstants.ProtocolVersion)
        {
            var message = new Chat($"Incompatible Minecraft client, protocol version required: {ServerConstants.ProtocolVersion}").ToString().ToVarString();
            using var session = command.Client.SocketWrapper.StartWriting();
            await session.WriteVarIntAsync(message.Length + 1);
            await session.WriteVarIntAsync(0x00);
            await session.WriteBytesAsync(message);
        }
        else
        {
            //TODO Implement encryption/compression
            var username = command.Client.Username.ToVarString();
            using var session = command.Client.SocketWrapper.StartWriting();
            await session.WriteVarIntAsync(1 + 16 + username.Length + 1);
            await session.WriteVarIntAsync(0x02);
            await session.WriteGuidAsync(command.Client.Id.Value);
            await session.WriteBytesAsync(username);
            await session.WriteVarIntAsync(0);
        }
    }
}
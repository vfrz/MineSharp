using MineSharp.Core.Packets;
using MineSharp.Network;

namespace MineSharp.Packets.Handlers;

public class ChatMessageHandler : IPacketHandler<ChatMessage>
{
    private readonly MinecraftServer _server;

    public ChatMessageHandler(MinecraftServer server)
    {
        _server = server;
    }

    public async ValueTask HandleAsync(ChatMessage command, CancellationToken cancellationToken)
    {
        await _server.BroadcastMessageAsync($"[{command.Client.Username}] {command.Message}");
    }
}
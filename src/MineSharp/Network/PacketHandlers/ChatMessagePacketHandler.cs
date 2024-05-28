using MineSharp.Commands;
using MineSharp.Network.Packets;

namespace MineSharp.Network.PacketHandlers;

public class ChatMessagePacketHandler : IClientPacketHandler<ChatMessagePacket>
{
    private readonly CommandHandler _commandHandler;

    public ChatMessagePacketHandler(CommandHandler commandHandler)
    {
        _commandHandler = commandHandler;
    }

    public async Task HandleAsync(ChatMessagePacket packet, ClientPacketHandlerContext context)
    {
        if (packet.Message.StartsWith('/'))
        {
            await _commandHandler.TryExecuteCommandAsync(packet.Message, context.Server, context.RemoteClient);
        }
        else
        {
            await context.Server.BroadcastChatAsync($"<{context.RemoteClient.Player!.Username}> {packet.Message}");
        }
    }
}
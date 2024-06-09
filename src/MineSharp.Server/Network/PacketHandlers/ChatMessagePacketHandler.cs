using MineSharp.Network.Packets;

namespace MineSharp.Network.PacketHandlers;

public class ChatMessagePacketHandler : IClientPacketHandler<ChatMessagePacket>
{
    public async Task HandleAsync(ChatMessagePacket packet, ClientPacketHandlerContext context)
    {
        if (packet.Message.StartsWith('/'))
        {
            await context.Server.CommandManager.TryExecuteCommandAsync(packet.Message, context.Server, context.RemoteClient);
        }
        else
        {
            await context.Server.BroadcastChatAsync($"<{context.RemoteClient.Player!.Username}> {packet.Message}");
        }
    }
}
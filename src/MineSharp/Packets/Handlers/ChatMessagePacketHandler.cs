using MineSharp.Core.Packets;

namespace MineSharp.Packets.Handlers;

public class ChatMessagePacketHandler : IClientPacketHandler<ChatMessagePacket>
{
    public async Task HandleAsync(ChatMessagePacket packet, ClientPacketHandlerContext context)
    {
        if (packet.Message == "/id")
        {
            await context.RemoteClient.SendPacketAsync(new ChatMessagePacket
            {
                Message = $"Your entity id: {context.RemoteClient.Player!.EntityId}"
            });
        }
        else
        {
            await context.Server.BroadcastMessageAsync($"[{context.RemoteClient.Username}] {packet.Message}");
        }
    }
}
using MineSharp.Core.Packets;

namespace MineSharp.Packets.Handlers;

public class ChatMessagePacketHandler : IClientPacketHandler<ChatMessagePacket>
{
    public async Task HandleAsync(ChatMessagePacket packet, ClientPacketHandlerContext context)
    {
        //TODO Move command handling somewhere else
        if (packet.Message == "/id")
        {
            await context.RemoteClient.SendPacketAsync(new ChatMessagePacket
            {
                Message = $"Your entity id: {context.RemoteClient.Player!.EntityId}"
            });
        }
        else if (packet.Message.StartsWith("/time"))
        {
            var value = long.Parse(packet.Message.Split(" ").Last());
            await context.Server.BroadcastPacketAsync(new TimeUpdatePacket
            {
                Time = value
            });
        }
        else
        {
            await context.Server.BroadcastMessageAsync($"[{context.RemoteClient.Username}] {packet.Message}");
        }
    }
}
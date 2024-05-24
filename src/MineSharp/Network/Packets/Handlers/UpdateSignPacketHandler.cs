namespace MineSharp.Network.Packets.Handlers;

public class UpdateSignPacketHandler : IClientPacketHandler<UpdateSignPacket>
{
    public async Task HandleAsync(UpdateSignPacket packet, ClientPacketHandlerContext context)
    {
        await context.Server.BroadcastPacketAsync(new UpdateSignPacket
        {
            X = packet.X,
            Y = packet.Y,
            Z = packet.Z,
            Text1 = packet.Text1,
            Text2 = packet.Text2,
            Text3 = packet.Text3,
            Text4 = packet.Text4
        });
    }
}
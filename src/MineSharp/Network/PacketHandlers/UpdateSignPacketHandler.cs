using MineSharp.Core;
using MineSharp.Network.Packets;
using MineSharp.TileEntities;

namespace MineSharp.Network.PacketHandlers;

public class UpdateSignPacketHandler : IClientPacketHandler<UpdateSignPacket>
{
    public async Task HandleAsync(UpdateSignPacket packet, ClientPacketHandlerContext context)
    {
        var signTileEntity = (await context.Server.World.GetTileEntityAsync<SignTileEntity>(new Vector3i(packet.X, packet.Y, packet.Z)))!;
        signTileEntity.Text1 = packet.Text1;
        signTileEntity.Text2 = packet.Text2;
        signTileEntity.Text3 = packet.Text3;
        signTileEntity.Text4 = packet.Text4;

        //TODO Send only for players that have the corresponding chunk loaded
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
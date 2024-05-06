namespace MineSharp.Core.Packets;

public interface IClientPacketHandler<in T> where T : IClientPacket
{
    Task HandleAsync(T packet, ClientPacketHandlerContext context);
}
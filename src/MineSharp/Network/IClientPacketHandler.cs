namespace MineSharp.Network;

public interface IClientPacketHandler<in T> : IClientPacketHandler where T : IClientPacket
{
    Task IClientPacketHandler.HandleAsync(IClientPacket packet, ClientPacketHandlerContext context) => HandleAsync((T) packet, context);

    public Task HandleAsync(T packet, ClientPacketHandlerContext context);
}

public interface IClientPacketHandler
{
    public Task HandleAsync(IClientPacket packet, ClientPacketHandlerContext context);
}
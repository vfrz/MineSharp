using MineSharp.Core.Packets;

namespace MineSharp.Packets.Handlers;

public class UnknownPacketHandler : IPacketHandler<UnknownPacket>
{
    public ValueTask HandleAsync(UnknownPacket command, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Unknown packet: 0x{command.PacketId:X}");
        return ValueTask.CompletedTask;
    }
}
using MineSharp.Core.Packets;

namespace MineSharp.Packets;

public class KeepAlivePacket : IServerPacket
{
    public const int Id = 0x00;

    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
    }
}
using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class HandshakeResponsePacket : IServerPacket
{
    public const byte Id = 0x02;

    public string ConnectionHash { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
        writer.WriteString(ConnectionHash);
    }
}
using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class HandshakeResponsePacket : IServerPacket
{
    public const byte Id = 0x02;

    public byte PacketId => Id;

    public string ConnectionHash { get; set; } = string.Empty;

    public void Write(PacketWriter writer)
    {
        writer.WriteString(ConnectionHash);
    }
}
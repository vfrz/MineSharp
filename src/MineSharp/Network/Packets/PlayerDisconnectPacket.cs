using System.Buffers;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class PlayerDisconnectPacket : IClientPacket, IServerPacket
{
    public const int Id = 0xFF;
    public byte PacketId => Id;

    public string Reason { get; set; } = string.Empty;

    public void Read(ref SequenceReader<byte> reader)
    {
        Reason = reader.ReadString();
    }

    public void Write(PacketWriter writer)
    {
        writer.WriteString(Reason);
    }
}
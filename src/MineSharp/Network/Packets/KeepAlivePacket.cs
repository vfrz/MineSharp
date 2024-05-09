using System.Buffers;
using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class KeepAlivePacket : IServerPacket, IClientPacket
{
    public const int Id = 0x00;

    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
    }

    public void Read(ref SequenceReader<byte> reader)
    {
    }
}
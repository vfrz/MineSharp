using System.Buffers;
using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class KeepAlivePacket : IServerPacket, IClientPacket
{
    public const int Id = 0x00;

    public byte PacketId => Id;

    public void Write(PacketWriter writer)
    {
    }

    public void Read(ref SequenceReader<byte> reader)
    {
    }
}
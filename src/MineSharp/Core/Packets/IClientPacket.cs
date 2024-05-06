using System.Buffers;

namespace MineSharp.Core.Packets;

public interface IClientPacket : IPacket
{
    void Read(ref SequenceReader<byte> reader);
}
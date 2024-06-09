using System.Buffers;

namespace MineSharp.Network;

public interface IClientPacket : IPacket
{
    void Read(ref SequenceReader<byte> reader);
}
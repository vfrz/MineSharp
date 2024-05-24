using System.Buffers;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class PlayerPacket : IClientPacket
{
    public const int Id = 0x0A;
    public byte PacketId => Id;

    public bool OnGround { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        OnGround = reader.ReadBool();
    }
}
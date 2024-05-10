using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class PlayerLookPacket : IClientPacket
{
    public const int Id = 0x0C;

    public byte PacketId => Id;

    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public bool OnGround { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        Yaw = reader.ReadFloat();
        Pitch = reader.ReadFloat();
        OnGround = reader.ReadBool();
    }
}
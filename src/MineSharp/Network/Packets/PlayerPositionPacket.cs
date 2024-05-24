using System.Buffers;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class PlayerPositionPacket : IClientPacket
{
    public const int Id = 0x0B;
    public byte PacketId => Id;

    public double X { get; set; }
    public double Y { get; set; }
    public double Stance { get; set; }
    public double Z { get; set; }
    public bool OnGround { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        X = reader.ReadDouble();
        Y = reader.ReadDouble();
        Stance = reader.ReadDouble();
        Z = reader.ReadDouble();
        OnGround = reader.ReadBool();
    }
}
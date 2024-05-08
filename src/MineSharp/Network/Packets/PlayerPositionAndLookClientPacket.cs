using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class PlayerPositionAndLookClientPacket : IClientPacket
{
    public const int Id = 0x0D;

    public double X { get; set; }
    public double Y { get; set; }
    public double Stance { get; set; }
    public double Z { get; set; }
    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public bool OnGround { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        X = reader.ReadDouble();
        Y = reader.ReadDouble();
        Stance = reader.ReadDouble();
        Z = reader.ReadDouble();
        Yaw = reader.ReadFloat();
        Pitch = reader.ReadFloat();
        OnGround = reader.ReadBool();
    }
}
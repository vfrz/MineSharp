using System.Buffers;
using MineSharp.Core;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public record PlayerDiggingPacket : IClientPacket
{
    public const int Id = 0x0E;
    public byte PacketId => Id;

    public PlayerDiggingStatus Status { get; set; }
    public int X { get; set; }
    public sbyte Y { get; set; }
    public int Z { get; set; }
    public sbyte Face { get; set; }

    public Vector3i PositionAsVector3i => new(X, Y, Z);

    public void Read(ref SequenceReader<byte> reader)
    {
        Status = (PlayerDiggingStatus) reader.ReadSByte();
        X = reader.ReadInt();
        Y = reader.ReadSByte();
        Z = reader.ReadInt();
        Face = reader.ReadSByte();
    }
}
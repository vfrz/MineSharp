using System.Buffers;
using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Packets;

public record PlayerDiggingPacket : IClientPacket
{
    public const int Id = 0x0E;
    
    public PlayerDiggingStatus Status { get; set; }
    public int X { get; set; }
    public sbyte Y { get; set; }
    public int Z { get; set; }
    public sbyte Face { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        Status = (PlayerDiggingStatus) reader.ReadSByte();
        X = reader.ReadInt();
        Y = reader.ReadSByte();
        Z = reader.ReadInt();
        Face = reader.ReadSByte();
    }
}
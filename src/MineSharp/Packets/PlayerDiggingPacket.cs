using System.Buffers;
using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Packets;

public record PlayerDiggingPacket : IClientPacket
{
    public const int Id = 0x0E;
    
    public PlayerDiggingStatus Status { get; private set; }
    public int X { get; private set; }
    public sbyte Y { get; private set; }
    public int Z { get; private set; }
    public sbyte Face { get; private set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        Status = (PlayerDiggingStatus) reader.ReadSByte();
        X = reader.ReadInt();
        Y = reader.ReadSByte();
        Z = reader.ReadInt();
        Face = reader.ReadSByte();
    }
}
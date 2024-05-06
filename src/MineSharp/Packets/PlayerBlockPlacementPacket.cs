using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Packets;

public class PlayerBlockPlacementPacket : IClientPacket
{
    public const int Id = 0x0F;
    
    public int X { get; private set; }
    public sbyte Y { get; private set; }
    public int Z { get; private set; }
    public sbyte Direction { get; private set; }
    public short BlockId { get; private set; }
    public byte? Amount { get; private set; }
    public short? Damage { get; private set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        X = reader.ReadInt();
        Y = reader.ReadSByte();
        Z = reader.ReadInt();
        Direction = reader.ReadSByte();
        BlockId = reader.ReadShort();
        Amount = BlockId != -1 ? reader.ReadByte() : null;
        Damage = BlockId != -1 ? reader.ReadShort() : null;
    }
}
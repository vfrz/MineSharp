using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class PlayerBlockPlacementPacket : IClientPacket
{
    public const int Id = 0x0F;
    
    public int X { get; set; }
    public sbyte Y { get; set; }
    public int Z { get; set; }
    public sbyte Direction { get; set; }
    public short BlockId { get; set; }
    public byte? Amount { get; set; }
    public short? Damage { get; set; }

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
using System.Buffers;
using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class PlayerBlockPlacementPacket : IClientPacket
{
    public const int Id = 0x0F;

    public byte PacketId => Id;

    public int X { get; set; }
    public sbyte Y { get; set; }
    public int Z { get; set; }
    public sbyte Direction { get; set; }
    public short ItemId { get; set; }
    public byte? Amount { get; set; }
    public short? Damage { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        X = reader.ReadInt();
        Y = reader.ReadSByte();
        Z = reader.ReadInt();
        Direction = reader.ReadSByte();
        ItemId = reader.ReadShort();
        Amount = ItemId != -1 ? reader.ReadByte() : null;
        Damage = ItemId != -1 ? reader.ReadShort() : null;
    }

    public Vector3i ToVector3i() => new(X, Y, Z);
}
using System.Buffers;
using MineSharp.Content;
using MineSharp.Core;
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
    public ItemId ItemId { get; set; }
    public byte? Amount { get; set; }
    public short? Metadata { get; set; }

    public Vector3<int> PositionAsVector3 => new(X, Y, Z);

    public void Read(ref SequenceReader<byte> reader)
    {
        X = reader.ReadInt();
        Y = reader.ReadSByte();
        Z = reader.ReadInt();
        Direction = reader.ReadSByte();
        ItemId = (ItemId) reader.ReadShort();
        Amount = ItemId != ItemId.Empty ? reader.ReadByte() : null;
        Metadata = ItemId != ItemId.Empty ? reader.ReadShort() : null;
    }
}
using MineSharp.Content;

namespace MineSharp.Network.Packets;

public class BlockUpdatePacket : IServerPacket
{
    public const byte Id = 0x35;
    public byte PacketId => Id;

    public int X { get; set; }
    public sbyte Y { get; set; }
    public int Z { get; set; }
    public BlockId BlockId { get; set; }
    public byte Metadata { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(X);
        writer.WriteSByte(Y);
        writer.WriteInt(Z);
        writer.WriteByte((byte)BlockId);
        writer.WriteByte(Metadata);
    }
}
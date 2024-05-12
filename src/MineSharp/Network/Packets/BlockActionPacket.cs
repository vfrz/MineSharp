using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class BlockActionPacket : IServerPacket
{
    public const byte Id = 0x36;
    public byte PacketId => Id;

    public int X { get; set; }
    public short Y { get; set; }
    public int Z { get; set; }
    public byte Data1 { get; set; }
    public byte Data2 { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(X);
        writer.WriteShort(Y);
        writer.WriteInt(Z);
        writer.WriteByte(Data1);
        writer.WriteByte(Data2);
    }
}
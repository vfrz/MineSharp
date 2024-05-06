using MineSharp.Core.Packets;

namespace MineSharp.Packets;

public class ChunkPacket : IServerPacket
{
    public const int Id = 0x33;

    public int X { get; set; }
    public short Y { get; set; }
    public int Z { get; set; }
    public byte SizeX { get; set; }
    public byte SizeY { get; set; }
    public byte SizeZ { get; set; }
    public byte[] CompressedData { get; set; }
    
    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
        writer.WriteInt(X);
        writer.WriteShort(Y);
        writer.WriteInt(Z);
        writer.WriteByte(SizeX);
        writer.WriteByte(SizeY);
        writer.WriteByte(SizeZ);
        writer.WriteInt(CompressedData.Length);
        writer.WriteBytes(CompressedData);
    }
}
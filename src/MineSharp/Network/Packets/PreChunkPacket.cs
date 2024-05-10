using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class PreChunkPacket : IServerPacket
{
    public const int Id = 0x32;

    public byte PacketId => Id;

    public int X { get; set; }

    public int Z { get; set; }

    public LoadingMode Mode { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(X);
        writer.WriteInt(Z);
        writer.WriteByte((byte) Mode);
    }

    public enum LoadingMode : byte
    {
        Unload = 0,
        Load = 1
    }
}
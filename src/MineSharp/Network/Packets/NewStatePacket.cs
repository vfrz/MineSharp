using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class NewStatePacket : IServerPacket
{
    public const int Id = 0x46;

    public ReasonType Reason { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
        writer.WriteByte((byte) Reason);
    }

    public enum ReasonType : byte
    {
        InvalidBed = 0,
        BeginRaining = 1,
        EndRaining = 2
    }
}
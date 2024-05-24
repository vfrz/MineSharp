namespace MineSharp.Network.Packets;

public class NewStatePacket : IServerPacket
{
    public const int Id = 0x46;
    public byte PacketId => Id;

    public ReasonType Reason { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteByte((byte) Reason);
    }

    public enum ReasonType : byte
    {
        InvalidBed = 0,
        BeginRaining = 1,
        EndRaining = 2
    }
}
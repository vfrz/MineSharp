namespace MineSharp.Network.Packets;

public class IncrementStatisticPacket : IServerPacket
{
    public const byte Id = 0xC8;
    public byte PacketId => Id;

    public int StatisticId { get; set; }
    public byte Amount { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(StatisticId);
        writer.WriteByte(Amount);
    }
}
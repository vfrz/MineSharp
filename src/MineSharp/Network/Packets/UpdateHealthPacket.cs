namespace MineSharp.Network.Packets;

public class UpdateHealthPacket : IServerPacket
{
    public const int Id = 0x08;
    public byte PacketId => Id;

    public short Health { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteShort(Health);
    }
}
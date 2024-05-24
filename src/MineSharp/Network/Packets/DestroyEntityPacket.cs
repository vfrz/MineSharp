namespace MineSharp.Network.Packets;

public class DestroyEntityPacket : IServerPacket
{
    public const int Id = 0x1D;
    public byte PacketId => Id;

    public int EntityId { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
    }
}
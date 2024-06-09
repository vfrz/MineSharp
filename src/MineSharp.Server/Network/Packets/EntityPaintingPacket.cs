namespace MineSharp.Network.Packets;

public class EntityPaintingPacket : IServerPacket
{
    public const byte Id = 0x19;
    public byte PacketId => Id;

    public int EntityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int Direction { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
        writer.WriteString(Title);
        writer.WriteInt(X);
        writer.WriteInt(Y);
        writer.WriteInt(Z);
        writer.WriteInt(Direction);
    }
}
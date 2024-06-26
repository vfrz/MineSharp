namespace MineSharp.Network.Packets;

public class EntityTeleportPacket : IServerPacket
{
    public const int Id = 0x22;
    public byte PacketId => Id;

    public int EntityId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public sbyte Yaw { get; set; }
    public sbyte Pitch { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
        writer.WriteInt(X);
        writer.WriteInt(Y);
        writer.WriteInt(Z);
        writer.WriteSByte(Yaw);
        writer.WriteSByte(Pitch);
    }
}
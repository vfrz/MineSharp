namespace MineSharp.Network.Packets;

public class EntityRelativeMovePacket : IServerPacket
{
    public const byte Id = 0x1F;
    public byte PacketId => Id;

    public int EntityId { get; set; }
    public sbyte AbsoluteX { get; set; }
    public sbyte AbsoluteY { get; set; }
    public sbyte AbsoluteZ { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
        writer.WriteSByte(AbsoluteX);
        writer.WriteSByte(AbsoluteY);
        writer.WriteSByte(AbsoluteZ);
    }
}
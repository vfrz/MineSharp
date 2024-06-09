namespace MineSharp.Network.Packets;

public class ItemDataPacket : IServerPacket
{
    public const byte Id = 0x83;
    public byte PacketId => Id;

    public short ItemType { get; set; }
    public short ItemId { get; set; }
    public byte[] Data { get; set; } = [];
    
    public void Write(PacketWriter writer)
    {
        writer.WriteShort(ItemType);
        writer.WriteShort(ItemId);
        writer.WriteByte((byte) Data.Length);
        writer.WriteBytes(Data);
    }
}
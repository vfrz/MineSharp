using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class OpenWindowPacket : IServerPacket
{
    public const int Id = 0x64;
    public byte PacketId => Id;

    public byte WindowId { get; set; }
    public byte InventoryType { get; set; }
    public string WindowTitle { get; set; } = string.Empty;
    public byte Slots { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteByte(WindowId);
        writer.WriteByte(InventoryType);
        writer.WriteString8(WindowTitle);
        writer.WriteByte(Slots);
    }
}
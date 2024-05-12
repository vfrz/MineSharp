using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class WindowClickPacket : IClientPacket
{
    public const int Id = 0x66;
    public byte PacketId => Id;

    public byte WindowId { get; set; }
    public short Slot { get; set; }
    public bool RightClick { get; set; }
    public short ActionNumber { get; set; }
    public bool Shift { get; set; }
    public short ItemId { get; set; }
    public byte ItemCount { get; set; }
    public short ItemUses { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        WindowId = reader.ReadByte();
        Slot = reader.ReadShort();
        RightClick = reader.ReadBool();
        ActionNumber = reader.ReadShort();
        Shift = reader.ReadBool();
        ItemId = reader.ReadShort();
        if (ItemId != -1)
        {
            ItemCount = reader.ReadByte();
            ItemUses = reader.ReadShort();
        }
    }
}
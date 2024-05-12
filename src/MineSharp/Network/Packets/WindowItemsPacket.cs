using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class WindowItemsPacket : IServerPacket
{
    public const byte Id = 0x68;
    public byte PacketId => Id;

    public byte WindowId { get; set; }
    public Item?[] Items { get; set; } = [];

    public void Write(PacketWriter writer)
    {
        writer.WriteByte(WindowId);
        writer.WriteShort((short) Items.Length);
        for (var i = 0; i < Items.Length; i++)
        {
            var item = Items[i];
            if (item.HasValue)
            {
                writer.WriteShort(item.Value.ItemId);
                writer.WriteByte(item.Value.Count);
                writer.WriteShort(item.Value.Uses);
            }
            else
            {
                writer.WriteShort(-1);
            }
        }
    }

    public record struct Item(short ItemId, byte Count, short Uses);
}
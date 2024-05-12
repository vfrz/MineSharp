using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class UpdateProgressBarPacket : IServerPacket
{
    public const byte Id = 0x69;
    public byte PacketId => Id;

    public byte WindowId { get; set; }
    public short ProgressBar { get; set; }
    public short Value { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteByte(WindowId);
        writer.WriteShort(ProgressBar);
        writer.WriteShort(Value);
    }
}
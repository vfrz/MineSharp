using System.Buffers;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class CloseWindowPacket : IClientPacket, IServerPacket
{
    public const int Id = 0x65;
    public byte PacketId => Id;

    public byte WindowId { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        WindowId = reader.ReadByte();
    }

    public void Write(PacketWriter writer)
    {
        writer.WriteByte(WindowId);
    }
}
using System.Buffers;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class TransactionPacket : IClientPacket, IServerPacket
{
    public const byte Id = 0x6A;
    public byte PacketId => Id;

    public byte WindowId { get; set; }
    public short ActionNumber { get; set; }
    public bool Accepted { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        WindowId = reader.ReadByte();
        ActionNumber = reader.ReadShort();
        Accepted = reader.ReadBool();
    }

    public void Write(PacketWriter writer)
    {
        writer.WriteByte(WindowId);
        writer.WriteShort(ActionNumber);
        writer.WriteBool(Accepted);
    }
}
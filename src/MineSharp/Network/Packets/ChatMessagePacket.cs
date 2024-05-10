using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class ChatMessagePacket : IClientPacket, IServerPacket
{
    public const int Id = 0x03;

    public byte PacketId => Id;

    public string Message { get; set; } = string.Empty;

    public void Read(ref SequenceReader<byte> reader)
    {
        Message = reader.ReadString();
    }

    public void Write(PacketWriter writer)
    {
        writer.WriteString(Message);
    }
}
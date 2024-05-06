using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Packets;

public class ChatMessagePacket : IClientPacket, IServerPacket
{
    public const int Id = 0x03;
    
    public string Message { get; set; }
    
    public void Read(ref SequenceReader<byte> reader)
    {
        Message = reader.ReadString();
    }

    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
        writer.WriteString(Message);
    }
}
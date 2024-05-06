using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Packets;

public class PlayerDisconnectPacket : IClientPacket, IServerPacket
{
    public const int Id = 0xFF;
    
    public string Reason { get; set; }
    
    public void Read(ref SequenceReader<byte> reader)
    {
        Reason = reader.ReadString();
    }

    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
        writer.WriteString(Reason);
    }
}
using System.Buffers;
using MineSharp.Core;
using MineSharp.Extensions;
using MineSharp.Sdk.Core;

namespace MineSharp.Network.Packets;

public class UpdateSignPacket : IClientPacket, IServerPacket
{
    public const byte Id = 0x82;
    public byte PacketId => Id;

    public int X { get; set; }
    public short Y { get; set; }
    public int Z { get; set; }
    public string Text1 { get; set; } = string.Empty;
    public string Text2 { get; set; } = string.Empty;
    public string Text3 { get; set; } = string.Empty;
    public string Text4 { get; set; } = string.Empty;

    public Vector3<int> PositionAsVector3 => new(X, Y, Z);

    public void Read(ref SequenceReader<byte> reader)
    {
        X = reader.ReadInt();
        Y = reader.ReadShort();
        Z = reader.ReadInt();
        Text1 = reader.ReadString();
        Text2 = reader.ReadString();
        Text3 = reader.ReadString();
        Text4 = reader.ReadString();
    }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(X);
        writer.WriteShort(Y);
        writer.WriteInt(Z);
        writer.WriteString(Text1);
        writer.WriteString(Text2);
        writer.WriteString(Text3);
        writer.WriteString(Text4);
    }
}
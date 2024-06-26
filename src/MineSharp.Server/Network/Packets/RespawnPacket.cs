using System.Buffers;
using MineSharp.Extensions;
using MineSharp.World;

namespace MineSharp.Network.Packets;

public class RespawnPacket : IClientPacket, IServerPacket
{
    public const int Id = 0x09;
    public byte PacketId => Id;

    public MinecraftDimension Dimension { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        Dimension = (MinecraftDimension) reader.ReadSByte();
    }

    public void Write(PacketWriter writer)
    {
        writer.WriteSByte((sbyte) Dimension);
    }
}
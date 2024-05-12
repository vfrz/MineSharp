using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class MultiBlockChangePacket : IServerPacket
{
    public const byte Id = 0x34;
    public byte PacketId => Id;

    public int ChunkX { get; set; }
    public int ChunkY { get; set; }
    public BlockChange[] Changes { get; set; } = [];

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(ChunkX);
        writer.WriteInt(ChunkY);
        writer.WriteShort((short) Changes.Length);

        foreach (var change in Changes)
        {
            //TODO Verify it's working correctly
            var position = (byte) (change.X << 4);
            position |= change.Z;
            writer.WriteByte(position);
            writer.WriteByte(change.Y);
        }

        foreach (var change in Changes)
            writer.WriteByte(change.BlockId);

        foreach (var change in Changes)
            writer.WriteByte(change.Metadata);
    }

    public record struct BlockChange(byte X, byte Y, byte Z, byte BlockId, byte Metadata);
}
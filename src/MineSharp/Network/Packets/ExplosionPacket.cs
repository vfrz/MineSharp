namespace MineSharp.Network.Packets;

public class ExplosionPacket : IServerPacket
{
    public const byte Id = 0x3C;
    public byte PacketId => Id;

    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public float Unknown { get; set; } //TODO Maybe radius
    public Record[] Records { get; set; } = [];

    public void Write(PacketWriter writer)
    {
        writer.WriteDouble(X);
        writer.WriteDouble(Y);
        writer.WriteDouble(Z);
        writer.WriteFloat(Unknown);
        writer.WriteInt(Records.Length);
        foreach (var record in Records)
        {
            writer.WriteSByte(record.X);
            writer.WriteSByte(record.Y);
            writer.WriteSByte(record.Z);
        }
    }

    public record struct Record(sbyte X, sbyte Y, sbyte Z);
}
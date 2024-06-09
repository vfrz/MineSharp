namespace MineSharp.Network.Packets;

public class PlayerPositionAndLookServerPacket : IServerPacket
{
    public const int Id = 0x0D;
    public byte PacketId => Id;

    public double X { get; set; }
    public double Stance { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public bool OnGround { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteDouble(X);
        writer.WriteDouble(Stance);
        writer.WriteDouble(Y);
        writer.WriteDouble(Z);
        writer.WriteFloat(Yaw);
        writer.WriteFloat(Pitch);
        writer.WriteBool(OnGround);
    }
}
namespace MineSharp.Core;

public class MinecraftPlayer
{
    public required int EntityId { get; set; }
    
    public required short Health { get; set; }

    public bool OnGround { get; set; }
    
    public double X { get; set; }
    
    public double Y { get; set; }
    
    public double Z { get; set; }
    
    public double Stance { get; set; }
    
    public float Yaw { get; set; }
    
    public float Pitch { get; set; }
    
    public bool PositionDirty { get; set; }
}
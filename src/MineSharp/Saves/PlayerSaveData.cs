using MineSharp.Core;
using MineSharp.Sdk.Core;

namespace MineSharp.Saves;

public class PlayerSaveData
{
    public Vector3<double> Position { get; set; }
    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public short Health { get; set; }
    public InventorySlotSaveData[] Inventory { get; set; } = [];
}
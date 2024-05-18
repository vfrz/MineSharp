using MineSharp.Core;

namespace MineSharp.Saves;

public class PlayerSaveData
{
    public Vector3d Position { get; set; }
    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public InventorySlotSaveData[] Inventory { get; set; } = [];
}
namespace MineSharp.Entities;

[Flags]
public enum EntityFlags
{
    Default = 0x00,
    Fire = 0x01,
    Crouched = 0x02,
    Riding = 0x04
}
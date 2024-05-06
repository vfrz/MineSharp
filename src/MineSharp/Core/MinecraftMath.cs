namespace MineSharp.Core;

public static class MinecraftMath
{
    public static sbyte RotationFloatToSByte(float rotation) => (sbyte) (rotation % 360 / 360 * 256);

    public static float SByteRotationToFloat(sbyte rotation) => rotation / 256f * 360;
}
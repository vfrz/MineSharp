namespace MineSharp.Core;

public static class MinecraftMath
{
    public static sbyte RotationFloatToSByte(float rotation) => (sbyte) (rotation % 360 / 360 * 256);

    public static float SByteRotationToFloat(sbyte rotation) => rotation / 256f * 360;

    public static int ToAbsoluteInt(this double value) => (int) (value * 32);

    public static double SinDegree(double degree) => Math.Sin(degree * (Math.PI / 180));

    public static double CosDegree(double degree) => Math.Cos(degree * (Math.PI / 180));
}
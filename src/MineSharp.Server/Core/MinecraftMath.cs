using MineSharp.Numerics;

namespace MineSharp.Core;

public static class MinecraftMath
{
    public static sbyte RotationFloatToSByte(float rotation) => (sbyte)(rotation % 360 / 360 * 256);

    public static float SByteRotationToFloat(sbyte rotation) => rotation / 256f * 360;

    public static int ToAbsolutePosition(this double value) => (int)(value * 32);

    public static int ToAbsolutePosition(this int value) => value * 32;

    public static Vector3<int> ToAbsolutePosition(this Vector3<int> position)
        => new(position.X.ToAbsolutePosition(), position.Y.ToAbsolutePosition(), position.Z.ToAbsolutePosition());

    public static double SinDegree(double degree) => Math.Sin(degree * (Math.PI / 180));

    public static double CosDegree(double degree) => Math.Cos(degree * (Math.PI / 180));
}
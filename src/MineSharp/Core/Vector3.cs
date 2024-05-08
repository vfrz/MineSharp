namespace MineSharp.Core;

public record struct Vector3(double X, double Y, double Z)
{
    public static readonly Vector3 One = new(1);

    public Vector3(double value) : this(value, value, value)
    {
    }

    public static Vector3 operator +(Vector3 left, Vector3 right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    public static Vector3 operator -(Vector3 left, Vector3 right)
        => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    public static Vector3 operator *(Vector3 left, Vector3 right)
        => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);

    public static Vector3 operator /(Vector3 left, Vector3 right)
        => new(left.X / right.X, left.Y / right.Y, left.Z / right.Z);

    public static Vector3 operator +(Vector3 vector) => vector;

    public static Vector3 operator -(Vector3 vector) => new(-vector.X, -vector.Y, -vector.Z);

    // ReSharper disable once InconsistentNaming
    public Vector3i ToVector3i() => new((int) X, (int) Y, (int) Z);
}
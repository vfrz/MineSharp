namespace MineSharp.Core;

public readonly struct Vector3d(double x, double y, double z)
{
    public static readonly Vector3d One = new(1);

    public readonly double X = x;

    public readonly double Y = y;

    public readonly double Z = z;

    public Vector3d(double value) : this(value, value, value)
    {
    }

    public static Vector3d operator +(Vector3d left, Vector3d right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    public static Vector3d operator -(Vector3d left, Vector3d right)
        => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    public static Vector3d operator *(Vector3d left, Vector3d right)
        => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);

    public static Vector3d operator /(Vector3d left, Vector3d right)
        => new(left.X / right.X, left.Y / right.Y, left.Z / right.Z);

    public static Vector3d operator +(Vector3d vector) => vector;

    public static Vector3d operator -(Vector3d vector) => new(-vector.X, -vector.Y, -vector.Z);

    // ReSharper disable once InconsistentNaming
    public Vector3i ToVector3i() => new((int) X, (int) Y, (int) Z);

    public override string ToString()
    {
        return $"{X}, {Y}, {Z}";
    }
}
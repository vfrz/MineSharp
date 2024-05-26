namespace MineSharp.Core;

// ReSharper disable once InconsistentNaming
public readonly struct Vector3i(int x, int y, int z)
{
    public static readonly Vector3i One = new(1);

    public readonly int X = x;

    public readonly int Y = y;

    public readonly int Z = z;

    public Vector3i(int value) : this(value, value, value)
    {
    }

    public static Vector3i operator +(Vector3i left, Vector3i right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    public static Vector3i operator -(Vector3i left, Vector3i right)
        => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    public static Vector3i operator *(Vector3i left, Vector3i right)
        => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);

    public static Vector3i operator /(Vector3i left, Vector3i right)
        => new(left.X / right.X, left.Y / right.Y, left.Z / right.Z);

    public static Vector3i operator +(Vector3i vector) => vector;

    public static Vector3i operator -(Vector3i vector) => new(-vector.X, -vector.Y, -vector.Z);

    public Vector3d ToVector3() => new(X, Y, Z);

    public Vector2i ToVector2i() => new(X, Z);
}
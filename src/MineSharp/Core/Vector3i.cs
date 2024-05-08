namespace MineSharp.Core;

// ReSharper disable once InconsistentNaming
public record struct Vector3i(int X, int Y, int Z)
{
    public static readonly Vector3i One = new(1);

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

    public Vector3 ToVector3() => new(X, Y, Z);

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }
}
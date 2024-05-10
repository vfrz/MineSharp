using System.Numerics;

namespace MineSharp.Core;

// ReSharper disable once InconsistentNaming
public record struct Vector2i(int X, int Z)
{
    public static readonly Vector2i Zero = new(0);

    public Vector2i(Vector2 vector) : this((int) vector.X, (int) vector.Y)
    {
    }

    public Vector2i(int value) : this(value, value)
    {
    }

    public static Vector2i operator +(Vector2i left, Vector2i right)
        => new(left.X + right.X, left.Z + right.Z);

    public static Vector2i operator -(Vector2i left, Vector2i right)
        => new(left.X - right.X, left.Z - right.Z);

    public static Vector2i operator *(Vector2i left, Vector2i right)
        => new(left.X * right.X, left.Z * right.Z);

    public static Vector2i operator /(Vector2i left, Vector2i right)
        => new(left.X / right.X, left.Z / right.Z);

    public static Vector2i operator +(Vector2i vector) => vector;

    public static Vector2i operator -(Vector2i vector) => new(-vector.X, -vector.Z);

    public override string ToString()
    {
        return $"{X}, {Z}";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Z);
    }
}
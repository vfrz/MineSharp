namespace MineSharp.Core;

// ReSharper disable once InconsistentNaming
public record struct Vector2i(int X, int Z)
{
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
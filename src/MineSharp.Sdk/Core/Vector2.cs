using System.Numerics;

namespace MineSharp.Sdk.Core;

public readonly struct Vector2<T>(T x, T z) : IEquatable<Vector2<T>> where T : INumber<T>
{
    public static readonly Vector2<T> Zero = new(T.CreateChecked(0));

    public static readonly Vector2<T> One = new(T.CreateChecked(1));

    public readonly T X = x;

    public readonly T Z = z;

    public Vector2(T value) : this(value, value)
    {
    }

    public static Vector2<T> CreateChecked<TOther>(Vector2<TOther> vector) where TOther : INumber<TOther>
    {
        return new Vector2<T>(T.CreateChecked(vector.X), T.CreateChecked(vector.Z));
    }

    public static Vector2<T> operator +(Vector2<T> left, Vector2<T> right)
        => new(left.X + right.X, left.Z + right.Z);

    public static Vector2<T> operator -(Vector2<T> left, Vector2<T> right)
        => new(left.X - right.X, left.Z - right.Z);

    public static Vector2<T> operator *(Vector2<T> left, Vector2<T> right)
        => new(left.X * right.X, left.Z * right.Z);

    public static Vector2<T> operator /(Vector2<T> left, Vector2<T> right)
        => new(left.X / right.X, left.Z / right.Z);

    public static Vector2<T> operator +(Vector2<T> vector) => vector;

    public static Vector2<T> operator -(Vector2<T> vector) => new(-vector.X, -vector.Z);

    public static bool operator ==(Vector2<T> left, Vector2<T> right) => left.Equals(right);

    public static bool operator !=(Vector2<T> left, Vector2<T> right) => !left.Equals(right);
    
    public static Vector2<T> operator +(Vector2<T> left, T right)
        => new(left.X + right, left.Z + right);

    public static Vector2<T> operator -(Vector2<T> left, T right)
        => new(left.X - right, left.Z - right);

    public static Vector2<T> operator *(Vector2<T> left, T right)
        => new(left.X * right, left.Z * right);

    public static Vector2<T> operator /(Vector2<T> left, T right)
        => new(left.X / right, left.Z / right);

    public double DistanceSquared(Vector2<T> other)
    {
        var dx = X - other.X;
        var dz = Z - other.Z;
        return Math.Sqrt(double.CreateChecked(dx * dx + dz * dz));
    }
    
    public override string ToString()
    {
        return $"{X}, {Z}";
    }

    public bool Equals(Vector2<T> other)
    {
        return X == other.X && Z == other.Z;
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Z);
    }
}
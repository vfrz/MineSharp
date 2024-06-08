using System.Numerics;

namespace MineSharp.Sdk.Core;

public readonly struct Vector3<T>(T x, T y, T z) : IEquatable<Vector3<T>> where T : INumber<T>
{
    public static readonly Vector3<T> Zero = new(T.CreateChecked(0));

    public static readonly Vector3<T> One = new(T.CreateChecked(1));

    public readonly T X = x;

    public readonly T Y = y;

    public readonly T Z = z;

    public Vector3(T value) : this(value, value, value)
    {
    }

    public static Vector3<T> CreateChecked<TOther>(Vector3<TOther> vector) where TOther : INumber<TOther>
        => new(T.CreateChecked(vector.X), T.CreateChecked(vector.Y), T.CreateChecked(vector.Z));

    public static Vector3<T> CreateChecked<TOther>(TOther x, TOther y, TOther z) where TOther : INumber<TOther>
        => new(T.CreateChecked(x), T.CreateChecked(y), T.CreateChecked(z));

    public Vector3<TOther> ToVector3<TOther>() where TOther : INumber<TOther>
        => new(TOther.CreateChecked(X), TOther.CreateChecked(Y), TOther.CreateChecked(Z));

    public static Vector3<T> operator +(Vector3<T> left, Vector3<T> right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    public static Vector3<T> operator -(Vector3<T> left, Vector3<T> right)
        => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    public static Vector3<T> operator *(Vector3<T> left, Vector3<T> right)
        => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);

    public static Vector3<T> operator /(Vector3<T> left, Vector3<T> right)
        => new(left.X / right.X, left.Y / right.Y, left.Z / right.Z);

    public static Vector3<T> operator +(Vector3<T> vector) => vector;

    public static Vector3<T> operator -(Vector3<T> vector) => new(-vector.X, -vector.Y, -vector.Z);

    public static bool operator ==(Vector3<T> left, Vector3<T> right) => left.Equals(right);

    public static bool operator !=(Vector3<T> left, Vector3<T> right) => !left.Equals(right);

    public static Vector3<T> operator +(Vector3<T> left, T right)
        => new(left.X + right, left.Y + right, left.Z + right);

    public static Vector3<T> operator -(Vector3<T> left, T right)
        => new(left.X - right, left.Y - right, left.Z - right);

    public static Vector3<T> operator *(Vector3<T> left, T right)
        => new(left.X * right, left.Y * right, left.Z * right);

    public static Vector3<T> operator /(Vector3<T> left, T right)
        => new(left.X / right, left.Y / right, left.Z / right);

    public double DistanceSquared(Vector3<T> other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        var dz = Z - other.Z;
        return Math.Sqrt(double.CreateChecked(dx * dx + dy * dy + dz * dz));
    }

    public override string ToString()
    {
        return $"{X}, {Y}, {Z}";
    }

    public bool Equals(Vector3<T> other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector3<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public Vector3<T> ApplyByteDirectionToPosition(sbyte direction)
    {
        if (direction == 0)
            return this + CreateChecked(0, -1, 0);
        if (direction == 1)
            return this + CreateChecked(0, 1, 0);
        if (direction == 2)
            return this + CreateChecked(0, 0, -1);
        if (direction == 3)
            return this + CreateChecked(0, 0, 1);
        if (direction == 4)
            return this + CreateChecked(-1, 0, 0);
        if (direction == 5)
            return this + CreateChecked(1, 0, 0);
        throw new Exception($"Unknown direction: {direction}");
    }
}
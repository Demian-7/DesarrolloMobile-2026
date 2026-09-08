using System;

public struct CellCoord : IEquatable<CellCoord>
{
    public int X;
    public int Y;

    public CellCoord(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(CellCoord other)
    {
        return (X == other.X && Y == other.Y);
    }

    public override bool Equals(object obj)
    {
        return obj is CellCoord other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (X * 397) ^ Y;
        }
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}
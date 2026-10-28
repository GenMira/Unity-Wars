using System;
using System.Collections.Generic;

public readonly struct HexPosition : IEquatable<HexPosition>
{
    public readonly int q;
    public readonly int r;

    public HexPosition(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    public static HexPosition operator +(HexPosition a, HexPosition b)
        => new HexPosition(a.q + b.q, a.r + b.r);
    public static HexPosition operator -(HexPosition a, HexPosition b)
        => new HexPosition(a.q - b.q, a.r - b.r);
    public static bool operator ==(HexPosition a, HexPosition b) => a.Equals(b);
    public static bool operator !=(HexPosition a, HexPosition b) => !a.Equals(b);
    public bool Equals(HexPosition other) => q == other.q && r == other.r;
    public override bool Equals(object obj) => obj is HexPosition other && Equals(other);
    public override int GetHashCode() => unchecked((q * 397) ^ r);
    public int abs() => (Math.Abs(q) + Math.Abs(r) + Math.Abs(q + r)) / 2;
    public bool normalized() => abs() == 1;
    public override string ToString() => $"({q},{r})";

    public List<HexPosition> GetAdjacent() => new List<HexPosition>
    {
        this + new HexPosition(1, 0), this + new HexPosition(0, 1),
        this + new HexPosition(-1, 1), this + new HexPosition(-1, 0),
        this + new HexPosition(0, -1), this + new HexPosition(1, -1)
    };
}

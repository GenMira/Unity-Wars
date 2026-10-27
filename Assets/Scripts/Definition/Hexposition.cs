using System;
public struct HexPosition
{
    public int q;
    public int r;

    public HexPosition(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    public static HexPosition operator +(HexPosition a, HexPosition b)
    {
        return new HexPosition(a.q + b.q, a.r + b.r);
    }

    public int abs()
    {
        return (Math.Abs(this.q) + Math.Abs(this.r) + Math.Abs(this.q + this.r)) / 2;
    }
    public bool normalized()
    {
        return this.abs() == 1;
    }
}
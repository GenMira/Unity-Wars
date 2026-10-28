using System;
using System.Collections.Generic;
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
    public static HexPosition operator -(HexPosition a, HexPosition b)
    {
        return new HexPosition(a.q - b.q, a.r - b.r);
    }

    public int abs()
    {
        return (Math.Abs(this.q) + Math.Abs(this.r) + Math.Abs(this.q + this.r)) / 2;
    }
    public bool normalized()
    {
        return this.abs() == 1;
    }

    public override string ToString()
    {
        return $"({q},{r})";
    }
    public List<HexPosition> GetAdjacent()
    {
        List<HexPosition> adjacent = new();
        for(int i = -1; i < 2; i++)
        {
            for(int j = -1;j < 2; j++)
            {
                HexPosition d = new HexPosition
                {
                    q=i,
                    r=j
                };

                if ((d - this).abs() == 1)
                {
                    adjacent.Add(d+this);
                }

                
            }
        }

        return adjacent;
    }
}
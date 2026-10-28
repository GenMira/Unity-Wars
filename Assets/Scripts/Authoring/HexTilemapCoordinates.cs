using System;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>Unityのodd-row offset座標とDomainのaxial座標の境界。</summary>
public static class HexTilemapCoordinates
{
    public static void Validate(Tilemap tilemap)
    {
        if (tilemap == null) throw new ArgumentNullException(nameof(tilemap));
        Grid grid = tilemap.layoutGrid;
        if (grid == null || grid.cellLayout != GridLayout.CellLayout.Hexagon)
            throw new ArgumentException($"六角GridのTilemapを指定してください: {tilemap.name}");
        if (grid.cellSwizzle != GridLayout.CellSwizzle.XYZ && grid.cellSwizzle != GridLayout.CellSwizzle.YXZ)
            throw new ArgumentException($"未対応のCell Swizzleです: {tilemap.name} / {grid.cellSwizzle}");
        if (tilemap.orientation != Tilemap.Orientation.XY)
            throw new ArgumentException($"TilemapのOrientationはXYを指定してください: {tilemap.name}");
    }

    public static HexPosition CellToHex(Tilemap tilemap, Vector3Int cell)
    {
        Validate(tilemap);
        if (cell.z != 0) throw new ArgumentException($"Z=0以外のセルには対応していません: {cell}");
        // 負の奇数行でも床関数になるよう、偶奇を引いてから2で割る。
        return new HexPosition(cell.x - (cell.y - (cell.y & 1)) / 2, cell.y);
    }

    public static Vector3Int HexToCell(Tilemap tilemap, HexPosition position)
    {
        Validate(tilemap);
        return new Vector3Int(position.q + (position.r - (position.r & 1)) / 2, position.r, 0);
    }

    public static Vector3 HexToWorld(Tilemap tilemap, HexPosition position)
        // Hexagonのセル原点は六角形の中心。Tile Anchorによる画像のずれは座標変換へ混ぜない。
        => tilemap.CellToWorld(HexToCell(tilemap, position));

    public static HexPosition WorldToHex(Tilemap tilemap, Vector3 worldPosition)
        => CellToHex(tilemap, tilemap.WorldToCell(worldPosition));
}

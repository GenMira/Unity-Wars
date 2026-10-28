using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

public sealed class MapExportResult
{
    public IReadOnlyList<string> Errors { get; }
    public IReadOnlyList<MapDefinitionAsset.TerrainCell> Cells { get; }
    public IReadOnlyList<MapDefinitionAsset.FacilityPlacementData> Facilities { get; }
    public bool Success => Errors.Count == 0;

    internal MapExportResult(List<string> errors, List<MapDefinitionAsset.TerrainCell> cells,
        List<MapDefinitionAsset.FacilityPlacementData> facilities)
    {
        Errors = errors.AsReadOnly();
        Cells = cells.AsReadOnly();
        Facilities = facilities.AsReadOnly();
    }
}

/// <summary>検証と書き込みを分け、失敗した書き出しでは既存アセットを変更しない。</summary>
public static class TilemapMapExporter
{
    public static MapExportResult Build(MapAuthoring source, bool newAsset = false)
    {
        List<string> errors = new();
        List<MapDefinitionAsset.TerrainCell> cells = new();
        List<MapDefinitionAsset.FacilityPlacementData> facilities = new();
        MapExportResult Result() => new MapExportResult(errors, cells, facilities);
        if (source == null) { errors.Add("MapAuthoringを指定してください。"); return Result(); }
        if (!Enum.IsDefined(typeof(MapTilemapMode), source.Mode)) errors.Add("書き出しモードが不正です。");
        if (source.TileCatalog == null) errors.Add("Tile Catalogを設定してください。");
        if (source.Terrain == null) errors.Add("Terrain Tilemapを設定してください。");
        if (errors.Count > 0) return Result();

        Dictionary<TileBase, MapTileCatalog.TileRule> rules = ReadCatalog(source.TileCatalog, errors);
        List<Tilemap> layers = new() { source.Terrain };
        if (source.Mode == MapTilemapMode.SeparateLayers)
        {
            if (source.NeutralFacilities != null) layers.Add(source.NeutralFacilities);
            if (source.RedFacilities != null) layers.Add(source.RedFacilities);
            if (source.BlueFacilities != null) layers.Add(source.BlueFacilities);
        }
        if (layers.Distinct().Count() != layers.Count) errors.Add("同じTilemapを複数のレイヤーへ指定できません。");
        foreach (Tilemap layer in layers) ValidateLayer(source.Terrain, layer, errors);
        if (errors.Count > 0) return Result();

        HashSet<HexPosition> terrainPositions = new();
        HashSet<HexPosition> facilityPositions = new();
        ReadTiles(source.Terrain, (cell, tile) =>
        {
            if (!TryGetRule(rules, source.Terrain, cell, tile, errors, out MapTileCatalog.TileRule rule)) return;
            HexPosition position = HexTilemapCoordinates.CellToHex(source.Terrain, cell);
            if (source.Mode == MapTilemapMode.SeparateLayers && rule.hasFacility)
            {
                errors.Add($"{source.Terrain.name} {cell}: 施設Tileは施設レイヤーへ配置してください。");
                return;
            }
            terrainPositions.Add(position);
            cells.Add(new MapDefinitionAsset.TerrainCell { q = position.q, r = position.r, areaType = rule.terrain });
            if (source.Mode == MapTilemapMode.Mixed && rule.hasFacility)
                AddFacility(source.Terrain, cell, position, rule, rule.isNeutral, rule.owner, facilities, facilityPositions, errors);
        }, errors);

        if (source.Mode == MapTilemapMode.SeparateLayers)
        {
            ReadFacilities(source.NeutralFacilities, true, PlayerSide.Red);
            ReadFacilities(source.RedFacilities, false, PlayerSide.Red);
            ReadFacilities(source.BlueFacilities, false, PlayerSide.Blue);
        }
        if (cells.Count == 0) errors.Add("地形Tileが1マスもありません。");
        cells.Sort((a, b) => Compare(a.q, a.r, b.q, b.r));
        facilities.Sort((a, b) => Compare(a.q, a.r, b.q, b.r));
        if (errors.Count == 0)
        {
            MapDefinitionAsset target = newAsset ? null : source.Output;
            try
            {
                MapDefinitionAsset.CreateDefinition(
                    target != null ? target.MapId : source.MapId,
                    target != null ? target.DisplayName : source.DisplayName,
                    cells.ToArray(), facilities.ToArray(),
                    target != null ? target.RedInitialMoney : source.RedInitialMoney,
                    target != null ? target.BlueInitialMoney : source.BlueInitialMoney);
            }
            catch (Exception error) { errors.Add(error.Message); }
        }
        return Result();

        void ReadFacilities(Tilemap layer, bool neutral, PlayerSide owner)
        {
            if (layer == null) return;
            ReadTiles(layer, (cell, tile) =>
            {
                if (!TryGetRule(rules, layer, cell, tile, errors, out MapTileCatalog.TileRule rule)) return;
                if (!rule.hasFacility) { errors.Add($"{layer.name} {cell}: 施設ではないTileです: {tile.name}"); return; }
                HexPosition position = HexTilemapCoordinates.CellToHex(layer, cell);
                if (!terrainPositions.Contains(position))
                { errors.Add($"{layer.name} {cell}: 地形のない場所に施設があります。"); return; }
                AddFacility(layer, cell, position, rule, neutral, owner, facilities, facilityPositions, errors);
            }, errors);
        }
    }

    public static MapExportResult UpdateExisting(MapAuthoring source)
    {
        MapExportResult result = Build(source);
        if (!result.Success) return result;
        if (source.Output == null || !AssetDatabase.Contains(source.Output))
            throw new InvalidOperationException("更新するMap Definitionアセットを指定してください。");
        Undo.RecordObject(source.Output, "Update Map Definition from Tilemap");
        source.Output.ReplaceLayout(result.Cells.ToArray(), result.Facilities.ToArray());
        Undo.FlushUndoRecordObjects();
        EditorUtility.SetDirty(source.Output);
        AssetDatabase.SaveAssetIfDirty(source.Output);
        return result;
    }

    public static MapDefinitionAsset CreateNew(MapAuthoring source, string assetPath)
    {
        MapExportResult result = Build(source, true);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Errors));
        string directory = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
        if (string.IsNullOrWhiteSpace(assetPath) || !assetPath.StartsWith("Assets/", StringComparison.Ordinal) ||
            assetPath.Contains("..") || Path.GetExtension(assetPath) != ".asset" ||
            !AssetDatabase.IsValidFolder(directory) || File.Exists(assetPath) ||
            AssetDatabase.LoadMainAssetAtPath(assetPath) != null)
            throw new ArgumentException("Assets内の、まだ存在しない.assetファイルを指定してください。", nameof(assetPath));

        MapDefinitionAsset asset = ScriptableObject.CreateInstance<MapDefinitionAsset>();
        try
        {
            asset.Initialize(source.MapId, source.DisplayName, result.Cells.ToArray(), result.Facilities.ToArray(),
                source.RedInitialMoney, source.BlueInitialMoney);
            AssetDatabase.CreateAsset(asset, assetPath);
        }
        catch { UnityEngine.Object.DestroyImmediate(asset); throw; }
        Undo.RegisterCreatedObjectUndo(asset, "Create Map Definition from Tilemap");
        Undo.RecordObject(source, "Assign Map Definition");
        source.SetOutput(asset);
        EditorUtility.SetDirty(source);
        if (source.gameObject.scene.IsValid()) EditorSceneManager.MarkSceneDirty(source.gameObject.scene);
        AssetDatabase.SaveAssetIfDirty(asset);
        return asset;
    }

    static int Compare(int aq, int ar, int bq, int br) => aq != bq ? aq.CompareTo(bq) : ar.CompareTo(br);

    static Dictionary<TileBase, MapTileCatalog.TileRule> ReadCatalog(MapTileCatalog catalog, List<string> errors)
    {
        Dictionary<TileBase, MapTileCatalog.TileRule> rules = new();
        if (catalog.Rules == null) { errors.Add("Tile CatalogのRulesが未設定です。"); return rules; }
        foreach (MapTileCatalog.TileRule rule in catalog.Rules)
        {
            if (rule == null || rule.tile == null) { errors.Add("Tile Catalogに未設定のTileがあります。"); continue; }
            if (!rules.TryAdd(rule.tile, rule)) errors.Add($"Tile Catalogに重複があります: {rule.tile.name}");
            if (!Enum.IsDefined(typeof(AreaType), rule.terrain)) errors.Add($"{rule.tile.name}: 地形種別が不正です。");
            if (rule.hasFacility && (rule.maxDurability <= 0 || !Enum.IsDefined(typeof(FacilityType), rule.facilityType) ||
                !Enum.IsDefined(typeof(PlayerSide), rule.owner))) errors.Add($"{rule.tile.name}: 施設の種類・耐久度・所有者が不正です。");
        }
        return rules;
    }

    static void ValidateLayer(Tilemap terrain, Tilemap layer, List<string> errors)
    {
        try { HexTilemapCoordinates.Validate(layer); }
        catch (Exception error) { errors.Add(error.Message); return; }
        if (terrain.layoutGrid != layer.layoutGrid || terrain.cellSize != layer.cellSize ||
            terrain.cellGap != layer.cellGap || terrain.cellSwizzle != layer.cellSwizzle ||
            terrain.tileAnchor != layer.tileAnchor ||
            !SameMatrix(terrain.transform.localToWorldMatrix, layer.transform.localToWorldMatrix))
            errors.Add($"{layer.name}: Terrainと同じGrid・Transform設定を使用してください。");
    }

    static bool SameMatrix(Matrix4x4 a, Matrix4x4 b)
    {
        for (int i = 0; i < 16; i++) if (!Mathf.Approximately(a[i], b[i])) return false;
        return true;
    }

    static void ReadTiles(Tilemap layer, Action<Vector3Int, TileBase> read, List<string> errors)
    {
        // cellBounds内の穴は出力しない。空セルは盤面外として残す。
        foreach (Vector3Int cell in layer.cellBounds.allPositionsWithin)
        {
            TileBase tile = layer.GetTile(cell);
            if (tile == null) continue;
            if (cell.z != 0) { errors.Add($"{layer.name} {cell}: Z=0以外のTileは書き出せません。"); continue; }
            read(cell, tile);
        }
    }

    static bool TryGetRule(Dictionary<TileBase, MapTileCatalog.TileRule> rules, Tilemap layer,
        Vector3Int cell, TileBase tile, List<string> errors, out MapTileCatalog.TileRule rule)
    {
        if (rules.TryGetValue(tile, out rule)) return true;
        errors.Add($"{layer.name} {cell}: 未登録のTileです: {tile.name}");
        return false;
    }

    static void AddFacility(Tilemap layer, Vector3Int cell, HexPosition position, MapTileCatalog.TileRule rule,
        bool neutral, PlayerSide owner, List<MapDefinitionAsset.FacilityPlacementData> facilities,
        HashSet<HexPosition> occupied, List<string> errors)
    {
        if (!occupied.Add(position)) { errors.Add($"{layer.name} {cell}: 施設が重複しています。"); return; }
        facilities.Add(new MapDefinitionAsset.FacilityPlacementData
        {
            q = position.q, r = position.r, type = rule.facilityType,
            isNeutral = neutral, owner = owner, maxDurability = rule.maxDurability
        });
    }
}

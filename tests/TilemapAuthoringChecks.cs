using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TilemapAuthoringChecks
{
    const string OutputPath = "Assets/GeneratedMap.asset";
    static int passed;
    static void Require(bool condition, string message = "Assertion failed")
    { if (!condition) throw new Exception(message); }
    static void Check(string name, Action check)
    {
        Undo.ClearAll();
        check();
        passed++;
        Debug.Log("MAPCHECK PASS " + name);
    }
    static void SetRef(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        var settings = new SerializedObject(target);
        settings.FindProperty(field).objectReferenceValue = value;
        settings.ApplyModifiedPropertiesWithoutUndo();
    }
    static void SetString(UnityEngine.Object target, string field, string value)
    {
        var settings = new SerializedObject(target);
        settings.FindProperty(field).stringValue = value;
        settings.ApplyModifiedPropertiesWithoutUndo();
    }
    static void SetInt(UnityEngine.Object target, string field, int value)
    {
        var settings = new SerializedObject(target);
        settings.FindProperty(field).intValue = value;
        settings.ApplyModifiedPropertiesWithoutUndo();
    }
    sealed class Fixture : IDisposable
    {
        public readonly GameObject Root;
        public readonly Grid Grid;
        public readonly MapAuthoring Source;
        public readonly Tilemap Terrain;
        public readonly Tilemap Neutral;
        public readonly Tilemap Red;
        public readonly Tilemap Blue;
        public readonly Tile Field;
        public readonly Tile City;
        public readonly MapTileCatalog Catalog;
        public Fixture(bool mixed = false)
        {
            Root = new GameObject("Authoring tests", typeof(Grid), typeof(MapAuthoring));
            Grid = Root.GetComponent<Grid>();
            Grid.cellLayout = GridLayout.CellLayout.Hexagon;
            Grid.cellSize = new Vector3(Mathf.Sqrt(3) / 2, 1, 1);
            Source = Root.GetComponent<MapAuthoring>();
            Terrain = Layer("Terrain"); Neutral = Layer("Neutral"); Red = Layer("Red"); Blue = Layer("Blue");
            Field = ScriptableObject.CreateInstance<Tile>(); Field.name = "Field";
            City = ScriptableObject.CreateInstance<Tile>(); City.name = "City";
            Catalog = ScriptableObject.CreateInstance<MapTileCatalog>();
            var settings = new SerializedObject(Catalog);
            var rules = settings.FindProperty("rules"); rules.arraySize = 2;
            SetRule(rules.GetArrayElementAtIndex(0), Field, false);
            SetRule(rules.GetArrayElementAtIndex(1), City, true);
            settings.ApplyModifiedPropertiesWithoutUndo();
            SetRef(Source, "tileCatalog", Catalog); SetRef(Source, "terrain", Terrain);
            SetRef(Source, "neutralFacilities", Neutral); SetRef(Source, "redFacilities", Red); SetRef(Source, "blueFacilities", Blue);
            SetInt(Source, "mode", mixed ? 0 : 1);
            SetString(Source, "mapId", "test-map"); SetString(Source, "displayName", "Test Map");
            SetInt(Source, "redInitialMoney", 500); SetInt(Source, "blueInitialMoney", 1000);
        }
        Tilemap Layer(string name)
        {
            var go = new GameObject(name, typeof(Tilemap)); go.transform.SetParent(Root.transform, false);
            return go.GetComponent<Tilemap>();
        }
        static void SetRule(SerializedProperty rule, Tile tile, bool facility)
        {
            rule.FindPropertyRelative("tile").objectReferenceValue = tile;
            rule.FindPropertyRelative("terrain").enumValueIndex = (int)AreaType.field;
            rule.FindPropertyRelative("hasFacility").boolValue = facility;
            rule.FindPropertyRelative("facilityType").enumValueIndex = (int)FacilityType.city;
            rule.FindPropertyRelative("maxDurability").intValue = 200;
            rule.FindPropertyRelative("isNeutral").boolValue = true;
            rule.FindPropertyRelative("owner").enumValueIndex = 0;
        }
        public void Dispose()
        {
            UnityEngine.Object.DestroyImmediate(Root);
            UnityEngine.Object.DestroyImmediate(Field); UnityEngine.Object.DestroyImmediate(City);
            UnityEngine.Object.DestroyImmediate(Catalog);
            Undo.ClearAll();
            AssetDatabase.DeleteAsset(OutputPath);
        }
    }

    public static void Run()
    {
        try
        {
            Check("hex coordinates round-trip and physical six neighbours in both swizzles", () => {
                using var f = new Fixture();
                foreach (var swizzle in new[] { GridLayout.CellSwizzle.XYZ, GridLayout.CellSwizzle.YXZ })
                {
                    f.Grid.cellSwizzle = swizzle;
                    for (int x = -4; x <= 4; x++) for (int y = -4; y <= 4; y++)
                    {
                        var cell = new Vector3Int(x, y, 0);
                        var hex = HexTilemapCoordinates.CellToHex(f.Terrain, cell);
                        Require(HexTilemapCoordinates.HexToCell(f.Terrain, hex) == cell, "cell round-trip");
                        var world = HexTilemapCoordinates.HexToWorld(f.Terrain, hex);
                        Require(HexTilemapCoordinates.WorldToHex(f.Terrain, world) == hex,
                            $"world round-trip: {swizzle} cell={cell} hex={hex} world={world} returned={f.Terrain.WorldToCell(world)} anchor={f.Terrain.tileAnchor}");
                        foreach (var neighbour in hex.GetAdjacent())
                            Require(Mathf.Abs(Vector3.Distance(world, HexTilemapCoordinates.HexToWorld(f.Terrain, neighbour)) - Mathf.Sqrt(3) / 2) < .001f, "physical adjacency");
                    }
                }
            });
            Check("mixed Tilemap emits terrain plus neutral facility and ignores empty bounds", () => {
                using var f = new Fixture(true);
                f.Terrain.SetTile(new Vector3Int(-2, -1, 0), f.Field);
                f.Terrain.SetTile(new Vector3Int(0, 1, 0), f.City);
                f.Terrain.SetTile(new Vector3Int(8, 8, 0), f.Field);
                f.Terrain.SetTile(new Vector3Int(8, 8, 0), null);
                var result = TilemapMapExporter.Build(f.Source);
                Require(result.Success, string.Join("\n", result.Errors));
                Require(result.Cells.Count == 2 && result.Facilities.Count == 1);
                Require(result.Facilities[0].isNeutral && result.Facilities[0].maxDurability == 200);
                Require(result.Cells.Any(c => c.q == -1 && c.r == -1));
            });
            Check("separate facility layers determine red blue neutral ownership", () => {
                using var f = new Fixture();
                for (int x = 0; x < 3; x++) f.Terrain.SetTile(new Vector3Int(x, 0, 0), f.Field);
                f.Neutral.SetTile(new Vector3Int(0, 0, 0), f.City);
                f.Red.SetTile(new Vector3Int(1, 0, 0), f.City);
                f.Blue.SetTile(new Vector3Int(2, 0, 0), f.City);
                var result = TilemapMapExporter.Build(f.Source);
                Require(result.Success, string.Join("\n", result.Errors));
                Require(result.Facilities[0].isNeutral);
                Require(!result.Facilities[1].isNeutral && result.Facilities[1].owner == PlayerSide.Red);
                Require(!result.Facilities[2].isNeutral && result.Facilities[2].owner == PlayerSide.Blue);
            });
            Check("unknown tile reports layer and cell", () => {
                using var f = new Fixture();
                var unknown = ScriptableObject.CreateInstance<Tile>(); unknown.name = "Unknown";
                try {
                    f.Terrain.SetTile(new Vector3Int(3, -2, 0), unknown);
                    var result = TilemapMapExporter.Build(f.Source);
                    Require(!result.Success && result.Errors.Any(e => e.Contains("Terrain") && e.Contains("Unknown") && e.Contains("3")));
                } finally { UnityEngine.Object.DestroyImmediate(unknown); }
            });
            Check("facility without terrain and overlapping ownership rejected", () => {
                using var f = new Fixture();
                f.Red.SetTile(Vector3Int.zero, f.City);
                Require(!TilemapMapExporter.Build(f.Source).Success);
                f.Terrain.SetTile(Vector3Int.zero, f.Field);
                f.Blue.SetTile(Vector3Int.zero, f.City);
                Require(TilemapMapExporter.Build(f.Source).Errors.Any(e => e.Contains("重複")));
            });
            Check("non hex grids shifted layers unsupported swizzle and z rejected", () => {
                using var f = new Fixture();
                f.Terrain.SetTile(Vector3Int.zero, f.Field);
                f.Grid.cellLayout = GridLayout.CellLayout.Rectangle;
                Require(!TilemapMapExporter.Build(f.Source).Success);
                f.Grid.cellLayout = GridLayout.CellLayout.Hexagon;
                f.Red.transform.localPosition = Vector3.right;
                Require(!TilemapMapExporter.Build(f.Source).Success);
                f.Red.transform.localPosition = Vector3.zero;
                f.Grid.cellSwizzle = GridLayout.CellSwizzle.XZY;
                Require(!TilemapMapExporter.Build(f.Source).Success);
                f.Grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;
                f.Terrain.SetTile(new Vector3Int(0, 0, 1), f.Field);
                Require(!TilemapMapExporter.Build(f.Source).Success);
            });
            Check("duplicate catalog rule and wrong layer tile rejected", () => {
                using var f = new Fixture();
                f.Terrain.SetTile(Vector3Int.zero, f.City);
                Require(!TilemapMapExporter.Build(f.Source).Success);
                f.Terrain.SetTile(Vector3Int.zero, f.Field);
                var catalog = new SerializedObject(f.Catalog);
                catalog.FindProperty("rules").GetArrayElementAtIndex(1).FindPropertyRelative("tile").objectReferenceValue = f.Field;
                catalog.ApplyModifiedPropertiesWithoutUndo();
                Require(TilemapMapExporter.Build(f.Source).Errors.Any(e => e.Contains("重複")));
            });
            Check("invalid facility terrain and empty map rejected", () => {
                using var f = new Fixture();
                Require(!TilemapMapExporter.Build(f.Source).Success);
                f.Terrain.SetTile(Vector3Int.zero, f.Field);
                f.Red.SetTile(Vector3Int.zero, f.City);
                var catalog = new SerializedObject(f.Catalog);
                catalog.FindProperty("rules").GetArrayElementAtIndex(0).FindPropertyRelative("terrain").enumValueIndex = (int)AreaType.sea;
                catalog.ApplyModifiedPropertiesWithoutUndo();
                Require(!TilemapMapExporter.Build(f.Source).Success);
            });
            Check("create update preserve metadata GUID thumbnail remove stale cells and undo redo", () => {
                using var f = new Fixture(true);
                f.Terrain.SetTile(Vector3Int.zero, f.Field); f.Terrain.SetTile(Vector3Int.right, f.City);
                var asset = TilemapMapExporter.CreateNew(f.Source, OutputPath);
                Require(asset.ToDefinition().Terrain.Cells.Count == 2 && f.Source.Output == asset);
                var guid = AssetDatabase.AssetPathToGUID(OutputPath);
                SetString(asset, "displayName", "Keep this title"); SetInt(asset, "redInitialMoney", 123);
                var texture = new Texture2D(2, 2);
                var sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.zero);
                AssetDatabase.CreateAsset(texture, "Assets/TestThumbnail.asset");
                AssetDatabase.AddObjectToAsset(sprite, texture);
                AssetDatabase.SaveAssetIfDirty(texture);
                SetRef(asset, "thumbnail", sprite);
                try {
                    f.Terrain.SetTile(Vector3Int.right, null);
                    SetString(f.Source, "mapId", "do-not-overwrite");
                    Undo.ClearAll();
                    Require(TilemapMapExporter.UpdateExisting(f.Source).Success);
                    Require(asset.ToDefinition().Terrain.Cells.Count == 1 && asset.ToDefinition().InitialFacilities.Count == 0);
                    Require(asset.MapId == "test-map" && asset.DisplayName == "Keep this title" && asset.RedInitialMoney == 123 && asset.BlueInitialMoney == 1000);
                    Require(asset.Thumbnail == sprite, "thumbnail reference preserved");
                    Require(AssetDatabase.AssetPathToGUID(OutputPath) == guid, "asset GUID preserved");
                    Undo.PerformUndo();
                    Require(asset.ToDefinition().Terrain.Cells.Count == 2 && asset.ToDefinition().InitialFacilities.Count == 1, "undo layout");
                    Undo.PerformRedo();
                    Require(asset.ToDefinition().Terrain.Cells.Count == 1, "redo layout");
                    var json = EditorJsonUtility.ToJson(asset);
                    Require(TilemapMapExporter.UpdateExisting(f.Source).Success);
                    Require(EditorJsonUtility.ToJson(asset) == json, "idempotent export");
                } finally { AssetDatabase.DeleteAsset("Assets/TestThumbnail.asset"); }
            });
            Check("failed update leaves asset unchanged", () => {
                using var f = new Fixture(true);
                f.Terrain.SetTile(Vector3Int.zero, f.Field);
                var asset = TilemapMapExporter.CreateNew(f.Source, OutputPath);
                var before = EditorJsonUtility.ToJson(asset);
                f.Terrain.ClearAllTiles();
                Require(!TilemapMapExporter.UpdateExisting(f.Source).Success);
                Require(EditorJsonUtility.ToJson(asset) == before);
            });
            Check("default catalog references existing field town and factory tiles", () => {
                var catalog = AssetDatabase.LoadAssetAtPath<MapTileCatalog>("Assets/Maps/DefaultMapTileCatalog.asset");
                Require(catalog != null && catalog.Rules.Count == 3 && catalog.Rules.All(r => r.tile != null));
                using var f = new Fixture(true);
                SetRef(f.Source, "tileCatalog", catalog);
                foreach (var rule in catalog.Rules) f.Terrain.SetTile(new Vector3Int(Array.IndexOf(catalog.Rules.ToArray(), rule), 0, 0), rule.tile);
                Require(TilemapMapExporter.Build(f.Source).Success);
            });
            Check("existing Battle scene mixed Tilemap exports without repainting", () => {
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");
                var tilemap = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Tilemap>()).Single();
                var source = tilemap.gameObject.AddComponent<MapAuthoring>();
                SetRef(source, "terrain", tilemap);
                SetRef(source, "tileCatalog", AssetDatabase.LoadAssetAtPath<MapTileCatalog>("Assets/Maps/DefaultMapTileCatalog.asset"));
                SetInt(source, "mode", (int)MapTilemapMode.Mixed);
                SetString(source, "mapId", "existing-battle");
                SetString(source, "displayName", "Existing Battle");
                var result = TilemapMapExporter.Build(source);
                Require(result.Success, string.Join("\n", result.Errors));
                Require(result.Cells.Count > 3 && result.Facilities.Count > 0);
            });
            Debug.Log($"MAPCHECK RESULT {passed} checks passed");
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogError("MAPCHECK FAIL " + error);
            EditorApplication.Exit(1);
        }
    }
}

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(MapAuthoring))]
public sealed class MapAuthoringEditor : Editor
{
    string message;
    MessageType messageType;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mode"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("tileCatalog"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("terrain"));
        if ((MapTilemapMode)serializedObject.FindProperty("mode").enumValueIndex == MapTilemapMode.SeparateLayers)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("neutralFacilities"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("redFacilities"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("blueFacilities"));
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("output"));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("新規作成時のメタデータ", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mapId"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("redInitialMoney"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("blueInitialMoney"));
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.HelpBox("Tile Paletteで描いた盤面から地形・施設を生成します。既存更新では出力先のID・名前・サムネイル・資金を保持します。", MessageType.Info);
        MapAuthoring source = (MapAuthoring)target;
        using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
        {
            if (GUILayout.Button("検証")) Run(() => Show(TilemapMapExporter.Build(source)));
            if (GUILayout.Button("新しいMap Definitionを作成")) Run(() =>
            {
                MapExportResult result = TilemapMapExporter.Build(source, true);
                if (!result.Success) { Show(result); return; }
                string path = EditorUtility.SaveFilePanelInProject("Map Definitionの保存先", "Map Definition", "asset", "新規アセットの保存先を指定してください。");
                if (string.IsNullOrEmpty(path)) return;
                MapDefinitionAsset asset = TilemapMapExporter.CreateNew(source, path);
                EditorGUIUtility.PingObject(asset);
                Show(result);
            });
            using (new EditorGUI.DisabledScope(source.Output == null))
            {
                if (GUILayout.Button("既存Map Definitionを更新")) Run(() => Show(TilemapMapExporter.UpdateExisting(source)));
            }
        }
        if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, messageType);
    }

    void Run(Action action)
    {
        try { action(); }
        catch (Exception error) { message = error.Message; messageType = MessageType.Error; }
    }

    void Show(MapExportResult result)
    {
        message = result.Success
            ? $"検証成功: 地形 {result.Cells.Count}マス、施設 {result.Facilities.Count}個。"
            : string.Join("\n", result.Errors);
        messageType = result.Success ? MessageType.Info : MessageType.Error;
    }

    [MenuItem("GameObject/Unity Wars/Map Authoring", false, 10)]
    static void CreateAuthoring(MenuCommand command)
    {
        GameObject root = new GameObject("Map Authoring", typeof(Grid), typeof(MapAuthoring));
        GameObjectUtility.SetParentAndAlign(root, command.context as GameObject);
        Undo.RegisterCreatedObjectUndo(root, "Create Map Authoring");
        Grid grid = root.GetComponent<Grid>();
        grid.cellLayout = GridLayout.CellLayout.Hexagon;
        grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;
        grid.cellSize = new Vector3(Mathf.Sqrt(3f) / 2f, 1f, 1f);
        MapAuthoring authoring = root.GetComponent<MapAuthoring>();
        SerializedObject settings = new SerializedObject(authoring);
        settings.FindProperty("terrain").objectReferenceValue = CreateLayer(root, "Terrain", 0);
        settings.FindProperty("neutralFacilities").objectReferenceValue = CreateLayer(root, "Neutral Facilities", 1);
        settings.FindProperty("redFacilities").objectReferenceValue = CreateLayer(root, "Red Facilities", 1);
        settings.FindProperty("blueFacilities").objectReferenceValue = CreateLayer(root, "Blue Facilities", 1);
        SetDefaultCatalog(settings);
        settings.ApplyModifiedProperties();
        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(root.scene);
    }

    static Tilemap CreateLayer(GameObject root, string name, int sortingOrder)
    {
        GameObject child = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
        child.transform.SetParent(root.transform, false);
        child.GetComponent<TilemapRenderer>().sortingOrder = sortingOrder;
        child.GetComponent<Tilemap>().tileAnchor = Vector3.zero;
        return child.GetComponent<Tilemap>();
    }

    [MenuItem("Tools/Unity Wars/Use Selected Tilemap as Mixed Map Source")]
    static void UseSelectedTilemap()
    {
        Tilemap tilemap = Selection.activeGameObject?.GetComponent<Tilemap>();
        if (tilemap == null) return;
        MapAuthoring authoring = tilemap.GetComponent<MapAuthoring>();
        if (authoring == null) authoring = Undo.AddComponent<MapAuthoring>(tilemap.gameObject);
        Undo.RecordObject(authoring, "Configure Mixed Map Source");
        SerializedObject settings = new SerializedObject(authoring);
        settings.FindProperty("mode").enumValueIndex = (int)MapTilemapMode.Mixed;
        settings.FindProperty("terrain").objectReferenceValue = tilemap;
        SetDefaultCatalog(settings);
        settings.ApplyModifiedProperties();
        Selection.activeGameObject = authoring.gameObject;
        EditorSceneManager.MarkSceneDirty(authoring.gameObject.scene);
    }

    [MenuItem("Tools/Unity Wars/Use Selected Tilemap as Mixed Map Source", true)]
    static bool HasSelectedTilemap() => Selection.activeGameObject?.GetComponent<Tilemap>() != null;

    static void SetDefaultCatalog(SerializedObject settings)
    {
        if (settings.FindProperty("tileCatalog").objectReferenceValue == null)
            settings.FindProperty("tileCatalog").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<MapTileCatalog>("Assets/Maps/DefaultMapTileCatalog.asset");
    }
}

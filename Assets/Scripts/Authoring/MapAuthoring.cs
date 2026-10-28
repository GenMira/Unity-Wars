using UnityEngine;
using UnityEngine.Tilemaps;

public enum MapTilemapMode
{
    Mixed,
    SeparateLayers
}

[DisallowMultipleComponent]
public sealed class MapAuthoring : MonoBehaviour
{
    [SerializeField] MapTilemapMode mode = MapTilemapMode.SeparateLayers;
    [SerializeField] MapTileCatalog tileCatalog;
    [Tooltip("Mixedでは施設Tileも含むTilemap。Separate Layersでは地形専用。")]
    [SerializeField] Tilemap terrain;
    [SerializeField] Tilemap neutralFacilities;
    [SerializeField] Tilemap redFacilities;
    [SerializeField] Tilemap blueFacilities;
    [SerializeField] MapDefinitionAsset output;
    [Header("新規アセット作成時の設定（既存更新時は出力先の設定を保持）")]
    [SerializeField] string mapId;
    [SerializeField] string displayName;
    [SerializeField, Min(0)] int redInitialMoney;
    [SerializeField, Min(0)] int blueInitialMoney;

    public MapTilemapMode Mode => mode;
    public MapTileCatalog TileCatalog => tileCatalog;
    public Tilemap Terrain => terrain;
    public Tilemap NeutralFacilities => neutralFacilities;
    public Tilemap RedFacilities => redFacilities;
    public Tilemap BlueFacilities => blueFacilities;
    public MapDefinitionAsset Output => output;
    public string MapId => mapId;
    public string DisplayName => displayName;
    public int RedInitialMoney => redInitialMoney;
    public int BlueInitialMoney => blueInitialMoney;

    public void SetOutput(MapDefinitionAsset asset) => output = asset;
}

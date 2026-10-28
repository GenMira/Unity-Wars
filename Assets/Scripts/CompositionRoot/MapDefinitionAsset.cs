using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unity Wars/Map Definition")]
public sealed class MapDefinitionAsset : ScriptableObject
{
    [Serializable]
    public struct TerrainCell
    {
        public int q;
        public int r;
        public AreaType areaType;
    }

    [Serializable]
    public struct FacilityPlacementData
    {
        public FacilityType type;
        public bool isNeutral;
        public PlayerSide owner;
        public int q;
        public int r;
        [Min(1)] public int maxDurability;
    }

    [SerializeField] string mapId;
    [SerializeField] string displayName;
    [SerializeField] Sprite thumbnail;
    [SerializeField] TerrainCell[] cells = Array.Empty<TerrainCell>();
    [SerializeField] FacilityPlacementData[] initialFacilities = Array.Empty<FacilityPlacementData>();
    [SerializeField, Min(0)] int redInitialMoney;
    [SerializeField, Min(0)] int blueInitialMoney;

    public string MapId => mapId;
    public Sprite Thumbnail => thumbnail;

    public MapDefinition ToDefinition()
    {
        if (cells == null  || initialFacilities == null)
            throw new InvalidOperationException($"マップの配列が未設定です: {name}");
        List<KeyValuePair<HexPosition, AreaType>> terrain = new();
        foreach (TerrainCell cell in cells)
            terrain.Add(new KeyValuePair<HexPosition, AreaType>(new HexPosition(cell.q, cell.r), cell.areaType));
        List<InitialFacilityDefinition> facilities = new();
        foreach (FacilityPlacementData facility in initialFacilities)
            facilities.Add(new InitialFacilityDefinition(facility.type,
                facility.isNeutral ? (PlayerSide?)null : facility.owner,
                new HexPosition(facility.q, facility.r), facility.maxDurability));
        return new MapDefinition(
            mapId, 
            displayName, 
            new BattleField(terrain),
            facilities, 
            redInitialMoney, 
            blueInitialMoney);
    }
}

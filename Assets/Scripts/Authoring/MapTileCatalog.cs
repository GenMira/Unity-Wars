using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Unity Wars/Map Tile Catalog")]
public sealed class MapTileCatalog : ScriptableObject
{
    [Serializable]
    public sealed class TileRule
    {
        public TileBase tile;
        public AreaType terrain;
        public bool hasFacility;
        public FacilityType facilityType;
        [Min(1)] public int maxDurability = 200;
        [Tooltip("地形・施設混在モードで使用する初期所有者。分離モードではレイヤーから決まります。")]
        public bool isNeutral = true;
        public PlayerSide owner;
    }

    [SerializeField] TileRule[] rules = Array.Empty<TileRule>();
    public IReadOnlyList<TileRule> Rules => rules;
}

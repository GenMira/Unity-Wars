using System;
using System.Collections.Generic;
using System.Linq;

public sealed class InitialFacilityDefinition
{
    public FacilityType Type { get; }
    public PlayerSide? Owner { get; }
    public HexPosition Position { get; }
    public int MaxDurability { get; }

    public InitialFacilityDefinition(FacilityType type, PlayerSide? owner,
        HexPosition position, int maxDurability)
    {
        if (!Enum.IsDefined(typeof(FacilityType), type)) throw new ArgumentOutOfRangeException(nameof(type));
        if (owner.HasValue && !Enum.IsDefined(typeof(PlayerSide), owner.Value))
            throw new ArgumentOutOfRangeException(nameof(owner));
        if (maxDurability <= 0) throw new ArgumentOutOfRangeException(nameof(maxDurability));
        Type = type;
        Owner = owner;
        Position = position;
        MaxDurability = maxDurability;
    }
}

/// <summary>再戦でも共有できる初期定義。対戦中のModelは含めない。</summary>
public sealed class MapDefinition
{
    public string MapId { get; }
    public string DisplayName { get; }
    public BattleField Terrain { get; }
    public IReadOnlyList<InitialFacilityDefinition> InitialFacilities { get; }
    public int RedInitialMoney { get; }
    public int BlueInitialMoney { get; }

    public MapDefinition(string mapId, string displayName, BattleField terrain,
        IEnumerable<InitialFacilityDefinition> initialFacilities,
        int redInitialMoney = 0, int blueInitialMoney = 0)
    {
        if (string.IsNullOrWhiteSpace(mapId)) throw new ArgumentException("マップIDが必要です。", nameof(mapId));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("マップ名が必要です。", nameof(displayName));
        Terrain = terrain ?? throw new ArgumentNullException(nameof(terrain));
        if (initialFacilities == null) throw new ArgumentNullException(nameof(initialFacilities));
        if (redInitialMoney < 0) throw new ArgumentOutOfRangeException(nameof(redInitialMoney));
        if (blueInitialMoney < 0) throw new ArgumentOutOfRangeException(nameof(blueInitialMoney));

        InitialFacilityDefinition[] facilityCopy = initialFacilities.ToArray();
        HashSet<HexPosition> occupiedTroops = new();
        HashSet<HexPosition> occupiedFacilities = new();
        foreach (InitialFacilityDefinition facility in facilityCopy)
        {
            if (facility == null) throw new ArgumentException("初期施設が未設定です。", nameof(initialFacilities));
            if (!terrain.Contains(facility.Position) || !occupiedFacilities.Add(facility.Position))
                throw new ArgumentException($"初期施設が盤面外、または重複しています: {facility.Position}");
            terrain.TryGetAreaType(facility.Position, out AreaType facilityArea);
            if ((facility.Type == FacilityType.headquarters || facility.Type == FacilityType.factory ||
                facility.Type == FacilityType.city) && facilityArea != AreaType.field)
                throw new ArgumentException($"陸上施設は平野に配置してください: {facility.Position}");
        }
        MapId = mapId;
        DisplayName = displayName;
        InitialFacilities = Array.AsReadOnly(facilityCopy);
        RedInitialMoney = redInitialMoney;
        BlueInitialMoney = blueInitialMoney;
    }
}

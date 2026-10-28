using System;
using System.Collections.Generic;

public sealed class MapCatalog
{
    readonly Dictionary<string, MapDefinition> maps = new(StringComparer.Ordinal);
    public IReadOnlyList<MapDefinition> Maps { get; }

    public MapCatalog(IEnumerable<MapDefinition> definitions)
    {
        if (definitions == null) throw new ArgumentNullException(nameof(definitions));
        List<MapDefinition> ordered = new();
        foreach (MapDefinition map in definitions)
        {
            if (map == null) throw new ArgumentException("マップが未設定です。", nameof(definitions));
            if (!maps.TryAdd(map.MapId, map))
                throw new ArgumentException($"マップIDが重複しています: {map.MapId}", nameof(definitions));
            ordered.Add(map);
        }
        if (ordered.Count == 0) throw new ArgumentException("マップを1つ以上登録してください。", nameof(definitions));
        Maps = ordered.AsReadOnly();
    }

    public MapDefinition Get(string mapId)
    {
        if (mapId == null) throw new ArgumentNullException(nameof(mapId));
        if (maps.TryGetValue(mapId, out MapDefinition map)) return map;
        throw new ArgumentException($"マップが登録されていません: {mapId}", nameof(mapId));
    }
}

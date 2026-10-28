using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>キーが存在する座標だけを盤面内とする、不変の地形データ。</summary>
public sealed class BattleField
{
    public IReadOnlyDictionary<HexPosition, AreaType> Cells { get; }

    public BattleField(IEnumerable<KeyValuePair<HexPosition, AreaType>> cells)
    {
        if (cells == null) throw new ArgumentNullException(nameof(cells));
        Dictionary<HexPosition, AreaType> copy = new();
        foreach (KeyValuePair<HexPosition, AreaType> cell in cells)
        {
            if (!Enum.IsDefined(typeof(AreaType), cell.Value))
                throw new ArgumentException($"未定義の地形です: {cell.Key} / {cell.Value}", nameof(cells));
            if (!copy.TryAdd(cell.Key, cell.Value))
                throw new ArgumentException($"地形の座標が重複しています: {cell.Key}", nameof(cells));
        }
        if (copy.Count == 0)
            throw new ArgumentException("地形を1マス以上指定してください。", nameof(cells));
        Cells = new ReadOnlyDictionary<HexPosition, AreaType>(copy);
    }

    public bool Contains(HexPosition position) => Cells.ContainsKey(position);
    public bool TryGetAreaType(HexPosition position, out AreaType areaType)
        => Cells.TryGetValue(position, out areaType);
}

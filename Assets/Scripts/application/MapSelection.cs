using System;

public sealed class MatchStartSettings
{
    public string MapId { get; }

    public MatchStartSettings(string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId)) throw new ArgumentException("マップIDが必要です。", nameof(mapId));
        MapId = mapId;
    }
}

/// <summary>選択画面と対戦画面の親スコープで共有する選択結果。</summary>
public sealed class MapSelection
{
    MatchStartSettings settings;
    public bool HasSelection => settings != null;

    public void Select(MatchStartSettings settings)
    {
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public MatchStartSettings GetSettings()
        => settings ?? throw new InvalidOperationException("対戦するマップを選択してください。");
}

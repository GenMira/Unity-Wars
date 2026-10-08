using System;

public static class UuidGenerator
{
    /// <summary>
    /// 新しいUUID（標準フォーマット: ハイフンあり小文字）を生成します。
    /// 例: "d3b07384-d113-40a1-a719-d36d57569326"
    /// </summary>
    public static string Generate()
    {
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    /// ハイフンなし（32桁の英数字）のUUIDを生成します。
    /// 例: "d3b07384d11340a1a719d36d57569326"
    /// </summary>
    public static string GenerateCompact()
    {
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// 大文字（ハイフンあり）のUUIDを生成します。
    /// 例: "D3B07384-D113-40A1-A719-D36D57569326"
    /// </summary>
    public static string GenerateUpper()
    {
        return Guid.NewGuid().ToString("D").ToUpperInvariant();
    }

    /// <summary>
    /// 指定された文字列が正しいUUID形式かどうかを検証します。
    /// </summary>
    public static bool IsValid(string uuidString)
    {
        if (string.IsNullOrWhiteSpace(uuidString))
            return false;

        return Guid.TryParse(uuidString, out _);
    }
}
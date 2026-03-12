using UnityEngine;
using System.Collections.Generic;

public static class ClearSaveManager
{
    public const string Stage = "StageClearData";
    public const string Adventure = "AdventureClearData";

    private static readonly Dictionary<string, AdventureClearSaveData> _cache = new();

    public static void Load(string key)
    {
        string json = PlayerPrefs.GetString(key, "");
        _cache[key] = string.IsNullOrEmpty(json)
            ? new AdventureClearSaveData()
            : AdventureClearSaveData.FromJson(json) ?? new AdventureClearSaveData();
    }

    public static void MarkCleared(string key, string stageId)
    {
        EnsureLoaded(key);
        if (!_cache[key].clearedStageIds.Contains(stageId))
        {
            _cache[key].clearedStageIds.Add(stageId);
            Save(key);
        }
    }

    public static bool IsCleared(string key, string stageId)
    {
        EnsureLoaded(key);
        return _cache[key].clearedStageIds.Contains(stageId);
    }

    private static void EnsureLoaded(string key)
    {
        if (!_cache.ContainsKey(key)) Load(key);
    }

    private static void Save(string key)
    {
        PlayerPrefs.SetString(key, _cache[key].ToJson());
        PlayerPrefs.Save();
    }
}

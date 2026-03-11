using UnityEngine;

public static class AdventureSaveManager
{
    private const string SaveKey = "AdventureClearData";
    private static AdventureClearSaveData _data;

    public static void Load()
    {
        string json = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(json))
            _data = new AdventureClearSaveData();
        else
            _data = AdventureClearSaveData.FromJson(json) ?? new AdventureClearSaveData();
    }

    public static void MarkCleared(string stageId)
    {
        if (_data == null) Load();
        if (!_data.clearedStageIds.Contains(stageId))
        {
            _data.clearedStageIds.Add(stageId);
            Save();
        }
    }

    public static bool IsCleared(string stageId)
    {
        if (_data == null) Load();
        return _data.clearedStageIds.Contains(stageId);
    }

    private static void Save()
    {
        PlayerPrefs.SetString(SaveKey, _data.ToJson());
        PlayerPrefs.Save();
    }
}

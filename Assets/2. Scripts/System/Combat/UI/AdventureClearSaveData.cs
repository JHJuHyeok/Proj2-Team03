using System;
using System.Collections.Generic;

[Serializable]
public class AdventureClearSaveData
{
    public List<string> clearedStageIds = new List<string>();

    public string ToJson() => UnityEngine.JsonUtility.ToJson(this);

    public static AdventureClearSaveData FromJson(string json)
        => UnityEngine.JsonUtility.FromJson<AdventureClearSaveData>(json);
}

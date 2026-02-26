using UnityEngine;
using System;
using System.Collections.Generic;

// 종합 성장 데이터
[System.Serializable]
public class GameData : ISavable
{
    public int level;                                           // 레벨
    public double currentExp;                                   // 현재 경험치
    public List<int> upgradeLevels = new();                     // 강화 레벨
    public List<int> growthLevels = new();                      // 성장 레벨
    public string advanceGrade;                                 // 승급 단계
    public Dictionary<string, Possesion> skillInfo = new();     // 스킬 현황
    public Dictionary<string, Possesion> equipInfo = new();     // 장비 현황
    public Dictionary<string, List<int>> buddyInfo = new();     // 동료 현황
    public Dictionary<string, applyOption> buddyOption = new(); // 각 동료 승급 옵션
    public List<int> gachaLevel = new();                        // 뽑기 레벨

    public long lastSaveTime { get; set; }           // 마지막 저장 시간

    // 게임 데이터 초기화
    public static GameData CreateDefault()
    {
        return new GameData
        {
            level = 1,
            currentExp = 0,
            upgradeLevels = new List<int> { 0, 0, 0, 0, 0, 0, 0 },
            growthLevels = new List<int> { 0, 0, 0, 0, 0, 0, 0 },
            advanceGrade = "",
            gachaLevel = new List<int> { 0, 0, 0 },
            lastSaveTime = DateTime.UtcNow.Ticks
        };
    }
}

[System.Serializable]
public class Possesion
{
    public int count;
    public int level;
}

[System.Serializable]
public class applyOption
{
    public int optionCount;
    public Dictionary<StatType, float> optionContents;
}
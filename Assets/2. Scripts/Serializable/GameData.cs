using UnityEngine;
using System;
using System.Collections.Generic;

// 세이브 데이터 클래스
[System.Serializable]
public class GameData : ISavable
{
    public int level;                                           // 레벨
    public double currentExp;                                   // 현재 경험치
    public List<int> upgradeLevels = new();                     // 강화 레벨
    public List<int> growthLevels = new();                      // 성장 레벨
    public string advanceGrade;                                 // 진화 단계
    public Dictionary<string, Possesion> skillInfo = new();     // 스킬 현황
    public Dictionary<string, Possesion> equipInfo = new();     // 장비 현황
    public Dictionary<string, List<int>> buddyInfo = new();     // 버디 현황
    public Dictionary<string, applyOption> buddyOption = new(); // 버디 장착 진화 옵션
    public List<int> gachaLevel = new();                        // 가차 레벨
    public string currentStageId;                               // 현재 진행중인 스테이지
    public string lastStageId;                                  // 가장 최근 클리어한 스테이지

    public long lastSaveTime { get; set; }           // 마지막 저장 시간

    // 게임 데이터 초기화
    public static GameData CreateDefault()
    {
        var data = new GameData
        {
            level = 1,
            currentExp = 0,
            upgradeLevels = new List<int> { 0, 0, 0, 0, 0, 0, 0 },
            growthLevels = new List<int> { 0, 0, 0, 0, 0, 0, 0 },
            advanceGrade = "",
            gachaLevel = new List<int> { 0, 0, 0 },
            lastSaveTime = DateTime.UtcNow.Ticks,
            currentStageId = "STG_01_01",
            lastStageId = "STG_01_01"
        };

        // [조민희] 기본 장비 데이터 추가 (신규 유저용)
        AddDefaultEquipment(data);

        return data;
    }

    /// <summary>
    /// [조민희] 신규 유저에게 기본 장비 지급
    /// </summary>
    private static void AddDefaultEquipment(GameData data)
    {
        // 기본 무기 지급 (Common 등급)
        data.equipInfo["WP_000"] = new Possesion { count = 5, level = 3 };   // 녹슨검
        data.equipInfo["WP_001"] = new Possesion { count = 3, level = 1 };   // 넓은검
        data.equipInfo["WP_002"] = new Possesion { count = 2, level = 5 };   // 장검
        data.equipInfo["WP_003"] = new Possesion { count = 1, level = 2 };   // 강철검

        // 기본 악세서리 지급 (Common 등급)
        data.equipInfo["AC_000"] = new Possesion { count = 5, level = 3 };   // 녹슨 팔찌
        data.equipInfo["AC_001"] = new Possesion { count = 3, level = 1 };   // 나태의 귀걸이
        data.equipInfo["AC_002"] = new Possesion { count = 2, level = 5 };   // 초조의 반지
        data.equipInfo["AC_003"] = new Possesion { count = 1, level = 2 };   // 오래된 팬던트

        Debug.Log($"[GameData] 기본 장비 지급 완료: 무기 4종, 악세서리 4종");
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

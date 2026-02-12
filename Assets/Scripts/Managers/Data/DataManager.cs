using UnityEngine;
using System;
using System.Collections.Generic;

public enum GameDataType
{
    Level,
    Upgrade,
    Growth,
    AdvanceGrade,
    Skill,
    Equip,
    Buddy,
    Gacha
}

// [태환] - 스테이지/지역 조회용 캐시 구현
public class DataManager : Singleton<DataManager>
{
    // 실시간 데이터 임시 보관
    public GameData currentSaveData { get; private set; }

    public GameDB<MonsterData, MonsterDataList> monsters = new();
    public GameDB<SkillData, SkillDataList> skills = new();
    public GameDB<EquipData, EquipDataList> weapons = new();
    public GameDB<EquipData, EquipDataList> accessories = new();
    
    // StageList actually contains AreaData
    public GameDB<AreaData, StageDataList> maps = new();

    // 스테이지 조회용 캐시 (O(1) 조회)
    private Dictionary<string, StageData> _stageCache = new();

    public static event Action<GameDataType, string> OnDataUpdated;

    public void Init(GameData data)
    {
        // 현재 데이터 초기화
        currentSaveData = data;

        Debug.Log("성장 데이터 동기화 완료");
    }

    public void LoadAllDatabase()
    {
        monsters.Load("Json/Monster/MonsterList");
        skills.Load("Json/Skill/SkillList");
        weapons.Load("Json/Equip/WeaponList");
        accessories.Load("Json/Equip/AccessorieList");
        
        maps.Load("Json/Stage/StageList");
        BuildStageCache();
        
        Debug.Log("데이터 로드 완료");
    }

    // 스테이지 캐시 구축
    private void BuildStageCache()
    {
        _stageCache.Clear();

        foreach (var area in maps.GetAll())
        {
            if (area.stageList == null) continue;
            
            foreach (var stage in area.stageList)
            {
                _stageCache[stage.id] = stage;
            }
        }
    }

    public StageData GetStage(string stageId)
    {
        _stageCache.TryGetValue(stageId, out StageData stage);
        return stage;
    }


}

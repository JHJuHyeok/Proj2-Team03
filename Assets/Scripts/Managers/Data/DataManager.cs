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
        
        Debug.Log("데이터 로드 완료");
    }

    public StageData GetStage(string stageId)
    {
        foreach (var area in maps.GetAll())
        {
            if (area.stageList != null)
            {
                foreach (var stage in area.stageList)
                {
                    if (stage.id == stageId)
                        return stage;
                }
            }
        }
        return null;
    }

    // 스테이지 ID로 해당 지역 데이터 반환
    public AreaData GetAreaByStageId(string stageId)
    {
        foreach (var area in maps.GetAll())
        {
            if (area.stageList != null)
            {
                foreach (var stage in area.stageList)
                {
                    if (stage.id == stageId)
                        return area;
                }
            }
        }
        return null;
    }
}

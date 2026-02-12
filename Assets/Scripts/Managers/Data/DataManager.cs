using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

public static class DataManager
{
    // 실시간 유저 데이터
    public static GameData CurrentSaveData { get; private set; }

    // 읽기 전용 데이터베이스
    public static readonly GameDB<MonsterData, MonsterDataList> monsters = new();
    public static readonly GameDB<SkillData, SkillDataList> skills = new();
    public static readonly GameDB<EquipData, EquipDataList> weapons = new();
    public static readonly GameDB<EquipData, EquipDataList> accessories = new();
    public static readonly GameDB<AreaData, StageDataList> stages = new();

    // UI 이벤트 전달
    public static event Action<GameDataType, string> OnDataUpdated;

    public static void Init(GameData data)
    {
        // 데이터 동기화
        CurrentSaveData = data;

        Debug.Log("성장 데이터 동기화 완료");
    }

    /// <summary>
    /// 비동기 데이터베이스 구성
    /// </summary>
    /// <returns></returns>
    public static async Task LoadAllDatabase()
    {
        await Task.WhenAll(
            monsters.LoadAsync("Json/Monster/MonsterList"),
            skills.LoadAsync("Json/Skill/SkillList"),
            weapons.LoadAsync("Json/Equip/WeaponList"),
            accessories.LoadAsync("Json/Equip/AccessorieList"),
            stages.LoadAsync("Json/Stage/StageList")
        );
        Debug.Log("데이터 로드 완료");
    }

    public static StageData GetStage(string stageId)
    {
        foreach (var area in stages.GetAll())
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
    public static AreaData GetAreaByStageId(string stageId)
    {
        foreach (var area in stages.GetAll())
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

    /// <summary>
    /// 데이터 타입별 이벤트 반환
    /// </summary>
    /// <param name="type"> 데이터 타입 </param>
    /// <param name="id"> 변경되는 요소 ID </param>
    public static void TriggerDataUpdate(GameDataType type, string id)
    {
        OnDataUpdated?.Invoke(type, id);
    }
}

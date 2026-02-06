using UnityEngine;
using System.Collections.Generic;
using BackEnd;
using Newtonsoft.Json;

public class DataManager : Singleton<DataManager>
{
    public GameDB<MonsterData, MonsterDataList> monsters = new();
    public GameDB<SkillData, SkillDataList> skills = new();
    public GameDB<EquipData, EquipDataList> weapons = new();
    public GameDB<EquipData, EquipDataList> accessories = new();
    
    // StageList actually contains AreaData
    public GameDB<AreaData, StageDataList> maps = new();
    
    public GameDB<PlayerStatsData, PlayerStatsDataList> playerStats = new();

    protected override void Awake()
    {
        base.Awake();
        LoadAllDatabase();
    }

    public void Init(string json)
    {
        GameData loadedData = JsonConvert.DeserializeObject<GameData>(json);
        Debug.Log("성장 데이터 동기화 완료");
    }

    private void LoadAllDatabase()
    {
        monsters.Load("Json/Monster/MonsterList");
        skills.Load("Json/Skill/SkillList");
        weapons.Load("Json/Equip/WeaponList");
        accessories.Load("Json/Equip/AccessorieList");
        
        maps.Load("Json/Stage/StageList");
        
        playerStats.Load("Json/Player/PlayerStatsList");
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
}

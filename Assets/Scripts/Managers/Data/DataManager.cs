using UnityEngine;
using System.Collections.Generic;
using BackEnd;
using Newtonsoft.Json;
using System;
using System.IO;

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

    private bool _isDirty = false;              // 데이터 변경 여부
    private float _saveTimer = 0f;
    private const float saveInterval = 300f;    // 자동 저장 간격 5분

    private void Update()
    {
        _saveTimer += Time.deltaTime;

        // 5분 간격으로 자동 저장
        if (_saveTimer >= saveInterval)
        {
            if (_isDirty)
            {
                SaveToRemote();
            }
            _saveTimer = 0f;
        }
    }

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

    /// <summary>
    /// 서버에 현재 데이터 원격 저장
    /// </summary>
    public async void SaveToRemote()
    {
        // 저장 시간 기록
        PrepareForSave();
        CurrencyManager.Instance.PrepareForSave();

        bool saveResult = await BackendManager.Instance.SaveDataAsync("UserSave", currentSaveData);
        bool saveCurrency = await BackendManager.Instance.SaveDataAsync("UserCurrency", CurrencyManager.Instance.currencySave);

        if (saveResult && saveCurrency)
        {
            _isDirty = false;
            _saveTimer = 0f;
        }
    }

    /// <summary>
    /// 로컬 저장소에 임시 저장(변경사항 발생 시 호출)
    /// </summary>
    public void SaveDataToLocal()
    {
        PrepareForSave();

        string json = JsonConvert.SerializeObject(currentSaveData);
        File.WriteAllText(Application.persistentDataPath + "/temp_save.json", json);
    }

    /// <summary>
    /// 저장 시간 기록
    /// </summary>
    public void PrepareForSave() =>
        currentSaveData.lastSaveTime = DateTime.UtcNow.Ticks;

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

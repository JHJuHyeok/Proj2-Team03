using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    public GameDB<MonsterData, MonsterDataList> monsters = new();
    public GameDB<SkillData, SkillDataList> skills = new();
    public GameDB<EquipData, EquipDataList> weapons = new();
    public GameDB<EquipData, EquipDataList> accessories = new();

    protected override void Awake()
    {
        base.Awake();
        LoadAllData();
    }

    private void LoadAllData()
    {
        monsters.Load("Json/Monster/MonsterList");
        skills.Load("Json/Skill/SkillList");
        weapons.Load("Json/Equip/WeaponList");
        accessories.Load("Json/Equip/AccessorieList");
        Debug.Log("데이터 로드 완료");
    }
}

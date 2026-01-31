using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EquipDataList
{
    public EquipType listType;
    public List<EquipData> equipList;
}

public enum EquipType
{
    Weapon,
    Accessorie
}

[System.Serializable]
public class EquipData
{
    public string id;                           // 장비 ID
    public string name;                         // 장비 명칭
    public string spriteName;                   // 이미지 이름

    public EquipGrade grade;                    // 장비 등급
    public ItemEffect equipEffect;              // 장착 효과
    public List<ItemEffect> holdEffects;        // 보유 효과 목록
}

public enum EquipGrade
{
    Common,
    Uncommon,
    Rare,
    Hero,
    Legend,
    Myth,
    Infinite
}

[System.Serializable]
public class ItemEffect
{
    public EffectType type;     // 효과 종류
    public float initValue;     // 초기 수치
    public float levelUpValue;  // 레벨 당 증가 수치
}

public enum EffectType
{
    AttackBoost,        // 공격력 증가 (%)
    CriticalDamage,     // 추가 치명타 데미지
    GoldGain,           // 추가 골드 획득량
    HealthBoost,        // 체력/체력 회복량 증가
    ManaBoost,          // 전체 마나/마나 회복량
    ExpGain             // 추가 경험치
}
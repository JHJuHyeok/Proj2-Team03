using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SkillDataList
{
    public List<SkillData> skillList;
}

[System.Serializable]
public class SkillData
{
    public string id;               // 스킬 ID
    public string name;             // 스킬 명칭
    public string spriteName;       // 이미지 이름
    public string explain;          // 설명 텍스트
    public string effect;           // 효과 텍스트

    public SkillGrade grade;        // 스킬 등급
    public SkillType type;          // 스킬 타입
    public SkillRequest request;    // 사용 조건
    public SkillElement element;    // 스킬 속성

    public int maxLevel;            // 최대 레벨
    public int needMp;              // 소모 MP
    public int initialRate;         // 기본 수치 값
    public float levelUpValue;      // 레벨 당 수치 상승값
}

public enum SkillGrade
{
    Common,
    Uncommon,
    Rare,
    Hero,
    Legend,
    Myth
}

public enum SkillType
{
    Active,
    Passive
}

public enum SkillRequest
{
    Cooldown,
    AttackCount
}

public enum SkillElement
{
    Fire,
    Wind,
    Water,
    Earth
}
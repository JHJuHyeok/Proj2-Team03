using UnityEngine;
using System.Collections.Generic;
using SlayerLegend.Skill.Data;

[System.Serializable]
public class SkillDataList : IDataList<SkillData>
{
    public List<SkillData> skillList;
    public List<SkillData> GetList() => skillList;
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
    public SkillRequest request;    // 요청 조건
    public SkillElement element;    // 스킬 속성

    public int maxLevel;            // 최대 레벨
    public int needMp;              // 소비 MP
    public int initialRate;         // 기본 수치 (%)
    public float levelUpValue;      // 레벨 당 수치 증가

    public SkillEffectData effectData;  // 조민희 추가: DoT/CC 효과 데이터
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

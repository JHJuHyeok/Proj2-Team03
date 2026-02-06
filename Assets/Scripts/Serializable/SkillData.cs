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
    public string id;               // ��ų ID
    public string name;             // ��ų ��Ī
    public string spriteName;       // �̹��� �̸�
    public string explain;          // ���� �ؽ�Ʈ
    public string effect;           // ȿ�� �ؽ�Ʈ

    public SkillGrade grade;        // ��ų ���
    public SkillType type;          // ��ų Ÿ��
    public SkillRequest request;    // ��� ����
    public SkillElement element;    // ��ų �Ӽ�

    public int maxLevel;            // �ִ� ����
    public int needMp;              // �Ҹ� MP
    public int initialRate;         // �⺻ ��ġ ��
    public float levelUpValue;      // ���� �� ��ġ ��°�

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
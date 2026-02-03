using UnityEngine;
using System.Collections.Generic;

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

    public int maxLevel;            // 최대 레벨
    public int needMp;              // 소비 MP
    public int initialRate;         // 기본 수치 %
    public float levelUpValue;      // 레벨업 수치 증가

    // DoT (Damage over Time) 관련
    public bool isDot;              // 도트 데미지 여부
    public float dotDuration;       // 도트 지속 시간
    public float dotDamagePerTick;  // 틱당 데미지
    public float dotTickInterval;   // 틱 간격
    public bool dotIsPercentage;    // 체력 비례 여부 (true면 % 데미지)

    // CC (Crowd Control) 관련
    public bool isStun;             // 기절 여부
    public float stunDuration;      // 기절 지속 시간
    public bool isFreeze;           // 빙결 여부
    public float freezeDuration;    // 빙결 지속 시간
    public bool isRoot;             // 속박 여부
    public float rootDuration;      // 속박 지속 시간
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
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EffectDto
{
    public string type;
    public float initValue;
    public float levelUpValue;
}

[Serializable]
public class EquipmentDto
{
    public string id;
    public string name;
    public string spriteName;
    public string grade;      // Common/Uncommon/Rare/Hero/Legend/Myth
    public int gradeStep;     // 1~4 등 (데이터 기준)
    public EffectDto equipEffect;
    public List<EffectDto> holdEffects;
}

[Serializable]
public class EquipmentListDto
{
    public string listType;         // Weapon / Accessorie
    public List<EquipmentDto> equipList;
}

[Serializable]
public class SkillEffectDataDto
{
    public int explosionRadius;
    public int hitCount;
    public float hitInterval;
    public bool isRandomHit;
    public int maxTargets;
    public bool isStun;
    public float stunDuration;
    public float stunChance;
    public bool isFreeze;
    public float freezeDuration;
    public float freezeChance;
    public float lastHitMultiplier;
}

[Serializable]
public class SkillDto
{
    public string id;
    public string name;
    public string spriteName;
    public string explain;
    public string effect;
    public string grade;
    public string type;          // Active/Passive
    public string request;       // AttackCount/Cooldown
    public string element;
    public int maxLevel;
    public float wantedDelay;
    public int needMp;
    public float initialRate;
    public float levelUpValue;
    public SkillEffectDataDto effectData;
}

[Serializable]
public class SkillListDto
{
    public List<SkillDto> skillList;
}

public enum EquipmentKind
{
    Weapon,
    Accessorie
}
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EquipDataList : IDataList<EquipData>
{
    public EquipType listType;
    public List<EquipData> equipList;
    public List<EquipData> GetList() => equipList;
}

public enum EquipType
{
    Weapon,
    Accessorie
}

[System.Serializable]
public class EquipData
{
    public string id;                           // ��� ID
    public string name;                         // ��� ��Ī
    public string spriteName;                   // �̹��� �̸�

    public EquipGrade grade;
    public int level = 1;  // 조민희 추가: 장비 레벨 (기본값 1)
    public ItemEffect equipEffect;              // ���� ȿ��
    public List<ItemEffect> holdEffects;        // ���� ȿ�� ���
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
    public EffectType type;     // ȿ�� ����
    public float initValue;     // �ʱ� ��ġ
    public float levelUpValue;  // ���� �� ���� ��ġ
}

public enum EffectType
{
    AttackBoost,        // ���ݷ� ���� (%)
    CriticalDamage,     // �߰� ġ��Ÿ ������
    GoldGain,           // �߰� ��� ȹ�淮
    HealthBoost,        // ü��/ü�� ȸ���� ����
    ManaBoost,          // ��ü ����/���� ȸ����
    ExpGain             // �߰� ����ġ
}
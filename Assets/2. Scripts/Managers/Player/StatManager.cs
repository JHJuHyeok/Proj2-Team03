using System;
using System.Collections.Generic;
using UnityEngine;

public class StatManager : Singleton<StatManager>
{
    // ���� ��� ��� ��Ʈ�ѷ�
    private StatController _playerStatController = new StatController();

    /// <summary>
    /// Controller�� �̺�Ʈ ����
    /// </summary>
    public event Action OnStatUpdated
    {
        add => _playerStatController.OnStatChanged += value;
        remove => _playerStatController.OnStatChanged -= value;
    }

    /// <summary>
    /// �ܺ� �ҽ�(���, ��ȭ ��)���� ���� ���� �� ȣ��
    /// </summary>
    public void UpdatePlayerStat(string sourceName, List<StatValue> stats)
    {
        _playerStatController.UpdateStatSource(sourceName, stats);
    }

    /// <summary>
    /// ���� �� ����
    /// </summary>
    /// <param name="type"> ���� Ÿ�� </param>
    /// <returns></returns>
    public double GetStat(StatType type) => _playerStatController.GetFinalStat(type);

    /// <summary>
    /// ����׿�: ���� ��� ���� ���� Ȯ��
    /// </summary>
    public void DebugAllStats()
    {
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            Debug.Log($"{type} : {GetStat(type)}");
        }
    }

    /// <summary>
    /// StatController 인스턴스 반환 (조민희 추가 - PlayerCombatStats 연동용)
    /// </summary>
    public StatController GetStatController() => _playerStatController;

    /// <summary>
    /// 저장 데이터로부터 업그레이드/성장 스탯 초기화
    /// </summary>
    public void InitFromSaveData(GameData data)
    {
        if (data == null) return;

        var upgradeStats = new List<StatValue>();
        if (data.upgradeLevels != null && data.upgradeLevels.Count >= 5)
        {
            upgradeStats.Add(new StatValue { type = StatType.STR,     baseValue = data.upgradeLevels[0] * 3 });
            upgradeStats.Add(new StatValue { type = StatType.HP,      baseValue = data.upgradeLevels[1] * 30 });
            upgradeStats.Add(new StatValue { type = StatType.VIT_HP,  baseValue = data.upgradeLevels[2] * 3 });
            upgradeStats.Add(new StatValue { type = StatType.CRI_Per, multiplier = data.upgradeLevels[3] * 0.1 });
            upgradeStats.Add(new StatValue { type = StatType.CRI_DMG, multiplier = data.upgradeLevels[4] * 1.0 });
        }

        var growthStats = new List<StatValue>();
        if (data.growthLevels != null && data.growthLevels.Count >= 7)
        {
            growthStats.Add(new StatValue { type = StatType.STR,      baseValue = data.growthLevels[0] * 5 });
            growthStats.Add(new StatValue { type = StatType.HP,       baseValue = data.growthLevels[1] * 30 });
            growthStats.Add(new StatValue { type = StatType.VIT_HP,   baseValue = data.growthLevels[2] * 5 });
            growthStats.Add(new StatValue { type = StatType.CRI_DMG,  multiplier = data.growthLevels[3] * 3.0 });
            growthStats.Add(new StatValue { type = StatType.ADD_GOLD, multiplier = data.growthLevels[4] * 0.5 });
            growthStats.Add(new StatValue { type = StatType.ACC,      baseValue = data.growthLevels[5] * 3 });
            growthStats.Add(new StatValue { type = StatType.DODGE,    baseValue = data.growthLevels[6] * 1 });
        }

        UpdatePlayerStat(SourceKey.Upgrade, upgradeStats.Count > 0 ? upgradeStats : null);
        UpdatePlayerStat(SourceKey.Growth,  growthStats.Count > 0  ? growthStats  : null);
    }
}
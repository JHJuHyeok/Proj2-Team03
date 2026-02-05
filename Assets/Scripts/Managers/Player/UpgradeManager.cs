using UnityEngine;
using System.Collections.Generic;

//===============강화 스크립트==============//

public class UpgradeManager : MonoBehaviour
{
    private Dictionary<StatType, int> _upgradeLevels = new();       // 각 스탯 레벨


    //☆☆☆ 강화 비용 테이블 Json 데이터 불러올 것 ☆☆☆//
    private UpgradeConfigList costList;



    // 스탯 레벨업
    public void UpgradeStat(StatType type)
    {
        // 레벨업
        _upgradeLevels[type]++;
        // 재화 소모
        CurrencyManager.Instance.ConsumeCurrency(CurrencyType.Gold, GetCost(type, _upgradeLevels[type]));

        List<StatValue> currentUpgradeStats = new();

        foreach (var level in _upgradeLevels)
        {
            StatUpgradeRow row = StatUpgradeTable.Table[level.Key];
            if (level.Value >= row.maxLevel) return;

            currentUpgradeStats.Add(new StatValue()
            {
                type = level.Key,
                baseValue = level.Value * row.perLevelValue,
                multiplier = level.Value * row.perLevelMul
            });
        }

        // 스탯 컨트롤러에 소스 갱신 요청
        StatController.Instance.UpdateStatSource(SourceKey.Upgrade, currentUpgradeStats);
    }

    /// <summary>
    /// 업그레이드 비용 산출
    /// </summary>
    /// <param name="type"> 스탯 타입 </param>
    /// <param name="level"> 스탯 레벨 </param>
    /// <returns></returns>
    private double GetCost(StatType type, int level)
    {
        UpgradeConfigData data = FindCostData(type);

        double value = data.startCost * Mathf.Pow(data.costIncreseRatio, level);

        return value;
    }

    // 업그레이드 비용 리스트 탐색
    private UpgradeConfigData FindCostData(StatType type)
    {
        foreach (var costData in costList.upgradeConfigs)
        {
            if (costData.type == type)
            {
                return costData;
            }
        }
        return null;
    }
}

public struct StatUpgradeRow
{
    public int maxLevel;
    public float perLevelValue;     // 합연산 증가량
    public float perLevelMul;       // 곱연산 증가량
}

// 스탯 업그레이드 수치
public static class StatUpgradeTable
{
    public static readonly Dictionary<StatType, StatUpgradeRow> Table = new()
        {
            {
                StatType.STR,
                new StatUpgradeRow()
                {
                    maxLevel = 2000000,
                    perLevelValue = 3,
                    perLevelMul = 0
                }
            },
            {
                StatType.HP,
                new StatUpgradeRow()
                {
                    maxLevel = 2000000,
                    perLevelValue = 30,
                    perLevelMul = 0
                }
            },
            {
                StatType.VIT_HP,
                new StatUpgradeRow()
                {
                    maxLevel = 2000000,
                    perLevelValue = 3,
                    perLevelMul = 0
                }
            },
            {
                StatType.CTI_DMG,
                new StatUpgradeRow()
                {
                    maxLevel = 10000,
                    perLevelValue = 0,
                    perLevelMul = 1f
                }
            },
            {
                StatType.CRI_Per,
                new StatUpgradeRow()
                {
                    maxLevel = 1000,
                    perLevelValue = 0,
                    perLevelMul = 0.1f
                }
            }
        };
}
using UnityEngine;
using System.Collections.Generic;

public static class SourceKey
{
    public const string Base = "Base";
    public const string Upgrade = "Char_Upgrade";
    public const string Growth = "Char_Growth";
    public const string Advance = "Char_Advance";
    public const string Equipment = "Equip";
    public const string Collect = "Equip_Collect";
    public const string Buddy = "Buddy";
}

public class StatController : Singleton<StatController>
{
    // 각 소스별 스탯 저장소
    private Dictionary<string, List<StatValue>> _statSources = new Dictionary<string, List<StatValue>>();
    // 최종 합산 스탯들
    private Dictionary<StatType, double> _finalStats = new Dictionary<StatType, double>();

    /// <summary>
    /// 스탯 소스 갱신
    /// </summary>
    /// <param name="sourceName"> 소스 구분 명칭 </param>
    /// <param name="stats"> 내부 스탯들 </param>
    public void UpdateStatSource(string sourceName, List<StatValue> stats)
    {
        _statSources[sourceName] = stats;
        RefreshFinalStats();
    }

    /// <summary>
    /// 최종 스탯 계산
    /// </summary>
    private void RefreshFinalStats()
    {
        _finalStats.Clear();

        // 타입별 일괄 계산
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            double sumBase = 0;
            double sumMultiplier = 1.0;

            // 각 소스의 type 별 계산
            foreach (var source in _statSources.Values)
            {
                var match = source.Find(s => s.type == type);
                if (match != null)
                {
                    sumBase += match.baseValue;
                    sumMultiplier += match.multiplier;
                }
            }

            _finalStats[type] = sumBase * sumMultiplier;
        }
    }

    /// <summary>
    /// 최종값 반환
    /// </summary>
    /// <param name="type"> 반환 스탯 </param>
    /// <returns> 값 반환 </returns>
    public double GetFinalStat(StatType type) => _finalStats.GetValueOrDefault(type, 0);
}


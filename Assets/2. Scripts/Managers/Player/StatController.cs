using UnityEngine;
using System;
using System.Collections.Generic;


// 저장소 명칭 정리(필요하면 추가해도 됩니다)
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

public class StatController
{
    // 각 소스별 스탯 저장소
    // Dictionary Key값 => SourceKey에 명시된 것을 이용할 것(오타 및 혼동 방지)
    private Dictionary<string, List<StatValue>> _statSources = new Dictionary<string, List<StatValue>>();
    // 최종 결과 임시 저장용 딕셔너리
    Dictionary<StatType, double> tempBase = new();
    Dictionary<StatType, double> tempMult = new();
    // 최종 합산 스탯 캐시
    private Dictionary<StatType, double> _finalStats = new();
    // 스탯 변경을 알리는 이벤트
    public Action OnStatChanged;

    /// <summary>
    /// 스탯 소스 갱신 및 전체 스탯 재계산
    /// </summary>
    /// <param name="sourceName"> 소스 구분 명칭 </param>
    /// <param name="stats"> 내부 스탯들 </param>
    public void UpdateStatSource(string sourceName, List<StatValue> stats)
    {
        if (stats == null)
        {
            _statSources.Remove(sourceName);
        }
        else
        {
            _statSources[sourceName] = stats;
        }

        RefreshFinalStats();
        OnStatChanged?.Invoke();    // 값 변경 알림
    }

    /// <summary>
    /// 최종 스탯 계산
    /// </summary>
    private void RefreshFinalStats()
    {
        tempBase.Clear();
        tempMult.Clear();

        // 각 소스의 type 별 계산
        foreach (var sourceList in _statSources.Values)
        {
            foreach (var stat in sourceList)
            {
                if (!tempBase.ContainsKey(stat.type))
                {
                    tempBase[stat.type] = 0;
                    tempBase[stat.type] += stat.baseValue;
                }

                if (!tempMult.ContainsKey(stat.type))
                {
                    tempMult[stat.type] = 0;
                    tempMult[stat.type] += stat.multiplier;
                }
            }
        }
        
        _finalStats.Clear();

        // 계산된 스탯 적용
        foreach (var type in tempBase.Keys)
        {
            _finalStats[type] = tempBase[type] * (1.0 + tempMult[type]);
        }
    }

    /// <summary>
    /// 최종값 반환
    /// </summary>
    /// <param name="type"> 반환 스탯 </param>
    /// <returns> 값 반환 </returns>
    public double GetFinalStat(StatType type) => _finalStats.GetValueOrDefault(type, 0);
}


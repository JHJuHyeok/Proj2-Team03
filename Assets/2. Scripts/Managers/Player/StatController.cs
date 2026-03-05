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
        _finalStats.Clear();

        // 타입별 일괄 계산
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            _finalStats.Clear();

            // 각 소스의 type 별 계산
            foreach (var sourceList in _statSources.Values)
            {
                foreach (var stat in sourceList)
                {
                    // 초기값이 없다면 기본 0 설정
                    if (!_finalStats.ContainsKey(stat.type))
                        _finalStats[stat.type] = 0;
                }
            }
        }

        // [조민희] 컬렉션 열거 중 수정 에러 방지를 위해 키 복사
        var types = new List<StatType>(_finalStats.Keys);
        foreach (var type in types)
        {
            double sumBase = 0;
            double sumMultiplier = 1.0f;

            foreach (var sourceList in _statSources.Values)
            {
                var match = sourceList.Find(s => s.type == type);
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


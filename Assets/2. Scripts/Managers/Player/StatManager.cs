using System;
using System.Collections.Generic;
using UnityEngine;

public class StatManager : Singleton<StatManager>
{
    // 실제 계산 담당 컨트롤러
    private StatController _playerStatController = new StatController();

    /// <summary>
    /// Controller의 이벤트 래핑
    /// </summary>
    public event Action OnStatUpdated
    {
        add => _playerStatController.OnStatChanged += value;
        remove => _playerStatController.OnStatChanged -= value;
    }

    /// <summary>
    /// 외부 소스(장비, 강화 등)에서 스탯 변경 시 호출
    /// </summary>
    public void UpdatePlayerStat(string sourceName, List<StatValue> stats)
    {
        _playerStatController.UpdateStatSource(sourceName, stats);
    }

    /// <summary>
    /// 스탯 값 참조
    /// </summary>
    /// <param name="type"> 스탯 타입 </param>
    /// <returns></returns>
    public double GetStat(StatType type) => _playerStatController.GetFinalStat(type);

    /// <summary>
    /// 디버그용: 현재 모든 최종 스탯 확인
    /// </summary>
    public void DebugAllStats()
    {
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            Debug.Log($"{type} : {GetStat(type)}");
        }
    }
}
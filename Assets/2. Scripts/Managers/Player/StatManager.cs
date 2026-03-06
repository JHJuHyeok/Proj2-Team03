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
}
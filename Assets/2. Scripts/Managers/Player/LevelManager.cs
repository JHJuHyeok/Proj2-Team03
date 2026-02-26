using UnityEngine;
using System;

public class LevelManager : Singleton<LevelManager>
{
    // 세이브 데이터 접근 프로퍼티
    private GameData _saveData => DataManager.CurrentSaveData;

    // 레벨 업 시 (새로운 레벨, 스탯 포인트) 전달
    public event Action<int> OnLevelUp;
    // 경험치 변동 시 (현재 경험치, 필요 경험치, 비율) 전달
    public event Action<double, double, float> OnExpChanged;

    /// <summary>
    /// 경험치 획득 메서드
    /// </summary>
    /// <param name="amount"> 획득 경험치량 </param>
    public void AddExp(double amount)
    {
        _saveData.currentExp += amount;

        CheckLevelUp();

        NotifyExpChanged();
    }

    private void CheckLevelUp()
    {
        double requiredExp = GetRequiredExp(_saveData.level);

        while (_saveData.currentExp >= requiredExp)
        {
            // 레벨업
            _saveData.currentExp -= requiredExp;
            _saveData.level++;

            // 스탯 포인트 획득
            int statPoints = 3;
            CurrencyManager.Instance.AddCurrency(CurrencyType.StatPoint, statPoints);

            // 레벨 변경 이벤트 실행
            OnLevelUp?.Invoke(_saveData.level);

            // 요구 경험치 재계산
            requiredExp = GetRequiredExp(_saveData.level);
        }
    }

    /// <summary>
    /// 레벨업 요구 경험치 공식
    /// </summary>
    /// <param name="level"> 플레이어 레벨 </param>
    /// <returns> 100 * 1.15의 레벨-1 제곱 </returns>
    public double GetRequiredExp(int level)
    {
        return 100 * Math.Pow(1.15, level - 1);
    }

    /// <summary>
    /// 경험치 이벤트 일괄 갱신
    /// </summary>
    public void NotifyExpChanged()
    {
        double req = GetRequiredExp(_saveData.level);
        float ratio = (float)(_saveData.currentExp / req);
        OnExpChanged?.Invoke(_saveData.currentExp, req, ratio);
    }
}

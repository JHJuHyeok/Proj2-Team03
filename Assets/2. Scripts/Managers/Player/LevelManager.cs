using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class LevelManager : Singleton<LevelManager>
{
    [Header("레벨업 버튼")]
    [SerializeField] private List<Button> LvUpButtons;

    // 세이브 데이터 접근 프로퍼티
    private GameData _saveData => DataManager.CurrentSaveData;

    private double _requiredExp;
    private bool _isCanLevelUp = false;

    // 레벨 업 시 (새로운 레벨, 스탯 포인트) 전달
    public event Action<int> OnLevelUp;
    // 경험치 변동 시 (현재 경험치, 필요 경험치, 비율) 전달
    public event Action<double, double, float> OnExpChanged;

    private void OnEnable()
    {
        // 활성화 시 필요 경험치 계산
        _requiredExp = GetRequiredExp(_saveData.level);

        if (LvUpButtons != null)
        {
            foreach (var button in LvUpButtons)
            {
                button.onClick.AddListener(LevelUp);
            }
        }
    }

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

    private bool CheckLevelUp()
    {
        if (_saveData.currentExp >= _requiredExp)
            _isCanLevelUp = true;
        else _isCanLevelUp = false;

        return _isCanLevelUp;
    }

    public void LevelUp()
    {
        if (_isCanLevelUp)
        {
            // 레벨업
            _saveData.currentExp -= _requiredExp;
            _saveData.level++;

            // 스탯 포인트 획득
            int statPoints = 3;
            CurrencyManager.Instance.AddCurrency(CurrencyType.StatPoint, statPoints);

            // 레벨 변경 이벤트 실행
            OnLevelUp?.Invoke(_saveData.level);

            // 요구 경험치 재계산
            _requiredExp = GetRequiredExp(_saveData.level);
        }
        else
        {
            Debug.Log("경험치가 부족해 레벨업이 불가능합니다.");
        }

        // 레벨업 가능 여부 체크
        if (!CheckLevelUp())
            _isCanLevelUp = false;
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

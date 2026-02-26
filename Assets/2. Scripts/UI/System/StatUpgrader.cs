using UnityEngine;
using System.Collections.Generic;
using TMPro;

// 어디서 사용될지 설정
public enum UsedTab
{
    Upgrade,
    Growth
}

public class StatUpgrader : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private StatType _targetStat;          // 성장시킬 스탯
    [SerializeField] private UsedTab _currentTab;           // 이 스크립트가 사용될 위치
    // GameData의 upgradeLevels 또는 growthLevels에서 변경시킬 스탯 인덱스
    [SerializeField] private int _upgradeIndex;

    [Header("UI 연결")]
    [SerializeField] private TMP_Text _levelText;           // 현재 레벨
    [SerializeField] private TMP_Text _valueText;           // 스탯 수치
    [SerializeField] private TMP_Text _costText;            // 강화 비용

    #region 업그레이드 별 재화 상승치
    private static float basicRate = 1.02f;
    private static float critDmgRate = 1.03f;
    private static float critPerRate = 1.1f;
    #endregion

    // 세이브 데이터 접근 프로퍼티
    private GameData _saveData => DataManager.CurrentSaveData;

    private int CurrentLevel
    {
        get
        {
            return _currentTab switch
            {
                UsedTab.Upgrade => _saveData.upgradeLevels[_upgradeIndex],
                UsedTab.Growth => _saveData.growthLevels[_upgradeIndex],
                _ => 0
            };
        }
        set
        {
            switch (_currentTab)
            {
                case UsedTab.Upgrade:
                    _saveData.upgradeLevels[_upgradeIndex] = value;
                    break;
                case UsedTab.Growth:
                    _saveData.growthLevels[_upgradeIndex] = value;
                    break;
            }
        }
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    /// <summary>
    /// 버튼에 부착할 업그레이드 스크립트
    /// </summary>
    public void TryUpgrade()
    {
        // 강화 칸에 적용 시
        if (_currentTab == UsedTab.Upgrade)
        {
            double cost = CalculateCost(_targetStat, CurrentLevel);

            // 재화 확인
            if (CurrencyManager.Instance.HasEnoughCurrency(CurrencyType.Gold, cost))
            {
                // 1. 재화가 충분하면 소모
                CurrencyManager.Instance.ConsumeCurrency(CurrencyType.Gold, cost);
                // 2. 레벨업
                CurrentLevel++;
                // 3. 스탯 컨트롤러에 변경사항 전달
                ApplyToStatController();
                // 4. UI 갱신
                RefreshUI();
            }
        }
        // 성장 탭에 적용 시
        else if (_currentTab == UsedTab.Growth)
        {
            if (CurrencyManager.Instance.GetAmount(CurrencyType.StatPoint) > 0)
            {
                CurrencyManager.Instance.ConsumeCurrency(CurrencyType.StatPoint, 1);

                CurrentLevel++;

                ApplyToStatController();
                RefreshUI();
            }
        }
    }

    /// <summary>
    /// 강화 탭에 적용 시 비용 계산 메서드
    /// </summary>
    /// <param name="stat"> 변경 스탯 타입 </param>
    /// <param name="level"> 각 업그레이드 칸의 레벨 </param>
    /// <returns> 필요 금액 </returns>
    private double CalculateCost(StatType stat, int level)
    {
        double cost;
        switch (stat)
        {
            case StatType.STR:
            case StatType.HP:
            case StatType.VIT_HP:
                cost = 100 * Mathf.Pow(basicRate, level);
                break;
            case StatType.CRI_DMG:
                cost = 100 * Mathf.Pow(critDmgRate, level);
                break;
            case StatType.CRI_Per:
                cost = 100 * Mathf.Pow(critPerRate, level);
                break;
            default:
                cost = 100 * Mathf.Pow(basicRate, level);
                break;
        }

        return cost;
    }

    private void RefreshUI()
    {
        if (_levelText != null)
            _levelText.text = $"Lv.{CurrentLevel}";
        if (_costText != null)
            _costText.text = CalculateCost(_targetStat, CurrentLevel).ToString("#,##0");
        if (_valueText != null)
            _valueText.text = $"{GetCurrentStatValue(CurrentLevel)}->{GetCurrentStatValue(CurrentLevel + 1)}";
    }

    private double GetCurrentStatValue(int level)
    {
        if (_currentTab == UsedTab.Upgrade)
        {
            switch (_targetStat)
            {
                case StatType.STR: return level * 3;
                case StatType.HP: return level * 30;
                case StatType.VIT_HP: return level * 3;
                case StatType.CRI_DMG: return level * 1;
                case StatType.CRI_Per: return level * 0.1f;
                default: return level * 1;
            }
        }
        else
        {
            switch (_targetStat)
            {
                case StatType.STR: return level * 5;
                case StatType.HP: return level * 30;
                case StatType.VIT_HP: return level * 5;
                case StatType.CRI_DMG: return level * 3;
                case StatType.ADD_GOLD: return level * 0.5;
                case StatType.ACC: return level * 3;
                case StatType.DODGE: return level * 1;
                default: return level * 1;
            }
        }
    }

    /// <summary>
    /// StatContoller에 변경된 값 전달
    /// </summary>
    private void ApplyToStatController()
    {
        double baseValue = 0;
        double multiplier = 0;

        if (_targetStat != StatType.CRI_DMG &&
            _targetStat != StatType.CRI_Per &&
            _targetStat != StatType.ADD_GOLD)
        {
            baseValue = GetCurrentStatValue(CurrentLevel);

            List<StatValue> stats = new List<StatValue>()
            {
                new StatValue{
                    type = _targetStat,
                    baseValue = baseValue,
                    multiplier = 0
                }
            };

            if (_currentTab ==  UsedTab.Upgrade)
                StatManager.Instance.UpdatePlayerStat(SourceKey.Upgrade, stats);
            else
                StatManager.Instance.UpdatePlayerStat(SourceKey.Growth, stats);
        }
        else
        {
            multiplier = GetCurrentStatValue(CurrentLevel);

            List<StatValue> stats = new List<StatValue>()
            {
                new StatValue{
                    type = _targetStat,
                    baseValue = 0,
                    multiplier = multiplier
                }
            };

            if (_currentTab == UsedTab.Upgrade)
                StatManager.Instance.UpdatePlayerStat(SourceKey.Upgrade, stats);
            else
                StatManager.Instance.UpdatePlayerStat(SourceKey.Growth, stats);
        }
    }
}

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
    // GameData의 upgradeLevels 또는 growthLevels에서 변경시킬 스탯 인덱스
    [SerializeField] private int _upgradeIndex;

    [Header("UI 연결")]
    [SerializeField] private EnhanceSlotUI _slotUI;         // 각 슬롯 정보

    #region 업그레이드 별 재화 상승치
    private static float basicRate = 1.02f;
    private static float critDmgRate = 1.03f;
    private static float critPerRate = 1.1f;
    #endregion

    // 세이브 데이터 접근 프로퍼티
    private GameData _saveData => DataManager.CurrentSaveData;

    // 각 업그레이드 레벨 참조 리스트 반환 메서드
    private List<int> GetTargetLevels(EnumUI.SlotKey key) => key switch
    {
        EnumUI.SlotKey.ENH_STR or
        EnumUI.SlotKey.ENH_HP or
        EnumUI.SlotKey.ENH_VIT or
        EnumUI.SlotKey.ENH_CRI_PROB or
        EnumUI.SlotKey.ENH_CRI_DMG
        => _saveData.upgradeLevels,

        EnumUI.SlotKey.GRO_STR or
        EnumUI.SlotKey.GRO_HP or
        EnumUI.SlotKey.GRO_VIT or
        EnumUI.SlotKey.GRO_CRI or
        EnumUI.SlotKey.GRO_LUK or
        EnumUI.SlotKey.GRO_ACC or
        EnumUI.SlotKey.GRO_DODGE
        => _saveData.growthLevels,

        _ => null
    };

    private int CurrentLevel
    {
        get
        {
            var array = GetTargetLevels(_slotUI.key);
            return array != null ? array[_upgradeIndex] : 0;
        }
        set
        {
            var array = GetTargetLevels(_slotUI.key);
            if (array != null) array[_upgradeIndex] = value;
        }
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    private EnumUI.SlotKey[] UpgradeEnums =
    {
        EnumUI.SlotKey.ENH_STR,
        EnumUI.SlotKey.ENH_HP,
        EnumUI.SlotKey.ENH_VIT,
        EnumUI.SlotKey.ENH_CRI_PROB,
        EnumUI.SlotKey.ENH_CRI_DMG
    };
    private EnumUI.SlotKey[] GrowthEnums =
    {
        EnumUI.SlotKey.GRO_STR,
        EnumUI.SlotKey.GRO_HP,
        EnumUI.SlotKey.GRO_VIT,
        EnumUI.SlotKey.GRO_CRI,
        EnumUI.SlotKey.GRO_LUK,
        EnumUI.SlotKey.GRO_ACC,
        EnumUI.SlotKey.GRO_DODGE
    };

    /// <summary>
    /// 버튼에 부착할 업그레이드 스크립트
    /// </summary>
    public void TryUpgrade()
    {
        // 강화 칸에 적용 시
        if (EnumUI.IsAny(_slotUI.key, UpgradeEnums))
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
        else if (EnumUI.IsAny(_slotUI.key, GrowthEnums))
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
    private long CalculateCost(StatType stat, int level)
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

        return (long)cost;
    }

    private void RefreshUI()
    {
        if (_slotUI != null)
        {
            _slotUI.SetLevel(CurrentLevel);
            _slotUI.SetCostGold(CalculateCost(_targetStat, CurrentLevel));
            _slotUI.SetValueChange(GetStatValue(CurrentLevel), GetStatValue(CurrentLevel + 1));
        }
    }

    private double GetStatValue(int level)
    {
        if (EnumUI.IsAny(_slotUI.key, UpgradeEnums))
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
            baseValue = GetStatValue(CurrentLevel);

            List<StatValue> stats = new List<StatValue>()
            {
                new StatValue{
                    type = _targetStat,
                    baseValue = baseValue,
                    multiplier = 0
                }
            };

            if (EnumUI.IsAny(_slotUI.key, UpgradeEnums))
                StatManager.Instance.UpdatePlayerStat(SourceKey.Upgrade, stats);
            else if (EnumUI.IsAny(_slotUI.key, GrowthEnums))
                StatManager.Instance.UpdatePlayerStat(SourceKey.Growth, stats);
        }
        else
        {
            multiplier = GetStatValue(CurrentLevel);

            List<StatValue> stats = new List<StatValue>()
            {
                new StatValue{
                    type = _targetStat,
                    baseValue = 0,
                    multiplier = multiplier
                }
            };

            if (EnumUI.IsAny(_slotUI.key, UpgradeEnums))
                StatManager.Instance.UpdatePlayerStat(SourceKey.Upgrade, stats);
            else if (EnumUI.IsAny(_slotUI.key, GrowthEnums))
                StatManager.Instance.UpdatePlayerStat(SourceKey.Growth, stats);
        }
    }
}

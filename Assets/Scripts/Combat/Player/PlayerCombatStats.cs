using UnityEngine;
using System;

// 플레이어의 전투 스탯을 관리하는 클래스
// StatController에서 최종 계산된 스탯을 가져오고, JSON에서 기본 스탯을 로드함
public class PlayerCombatStats : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private string playerStatsId = "Player_Default";

    // 기본 데이터 (StatType에 없는 스탯용)
    private PlayerStatsData _baseData;

    // 런타임 상태
    private double _currentHp;
    private double _currentMana;
    private bool _isDead = false;

    // 이벤트
    public event Action<double, double> OnHpChanged;
    public event Action<double, double> OnManaChanged;
    public event Action OnDeath;

    // === StatController에서 가져오는 스탯 ===
    public double MaxHealth => GetStatValue(StatType.HP);
    public double MaxMana => GetStatValue(StatType.MANA);
    public double AttackDamage => GetStatValue(StatType.STR);
    public double CriticalRate => GetStatValue(StatType.CRI_Per) / 100.0;
    public double CriticalDamage
    {
        get
        {
            double value = GetStatValue(StatType.CTI_DMG) / 100.0;
            return value > 0 ? value : 2.0; // 기본값 200%
        }
    }
    public double Accuracy => GetStatValue(StatType.ACC) / 100.0;
    public double Dodge => GetStatValue(StatType.DODGE) / 100.0;
    public double GoldGainPercent => GetStatValue(StatType.ADD_GOLD);
    public double ExpGainPercent => GetStatValue(StatType.ADD_EXP);
    private double HpRegenPerSecond => GetStatValue(StatType.VIT_HP);
    private double ManaRegenPerSecond => GetStatValue(StatType.VIT_MANA);

    // === 고정 스탯 (상수) ===
    public float AttackSpeed => 1f;
    public float AttackRange => 2f;
    public float DetectionRange => 15f;
    public float BlowDamage => _baseData?.blowDamage ?? 0f;
    public float BlowProbability => _baseData?.blowProbability ?? 0f;

    // === 런타임 상태 ===
    public double CurrentHealth => _currentHp;
    public double CurrentMana => _currentMana;
    public bool IsDead => _isDead;

    private void Start()
    {
        LoadBaseData();
        InitializeCurrentStats();
    }

    private void LoadBaseData()
    {
        if (DataManager.Instance == null)
        {
            Debug.LogError("[PlayerCombatStats] DataManager not found!");
            return;
        }

        _baseData = DataManager.Instance.playerStats.Get(playerStatsId);

        if (_baseData == null)
        {
            Debug.LogError($"[PlayerCombatStats] PlayerStatsData not found: {playerStatsId}");
        }
    }

    private void InitializeCurrentStats()
    {
        _currentHp = MaxHealth;
        _currentMana = MaxMana;
        _isDead = false;

        OnHpChanged?.Invoke(_currentHp, MaxHealth);
        OnManaChanged?.Invoke(_currentMana, MaxMana);
    }

    private void Update()
    {
        if (_isDead) return;

        // HP 회복
        if (_currentHp < MaxHealth && HpRegenPerSecond > 0)
        {
            _currentHp += HpRegenPerSecond * Time.deltaTime;
            _currentHp = Math.Min(_currentHp, MaxHealth);
            OnHpChanged?.Invoke(_currentHp, MaxHealth);
        }

        // Mana 회복
        if (_currentMana < MaxMana && ManaRegenPerSecond > 0)
        {
            _currentMana += ManaRegenPerSecond * Time.deltaTime;
            _currentMana = Math.Min(_currentMana, MaxMana);
            OnManaChanged?.Invoke(_currentMana, MaxMana);
        }
    }

    /// <summary>
    /// StatController에서 스탯 값 조회
    /// </summary>
    private double GetStatValue(StatType type)
    {
        if (StatController.Instance == null)
        {
            Debug.LogWarning("[PlayerCombatStats] StatController not initialized yet");
            return 0;
        }

        return StatController.Instance.GetFinalStat(type);
    }

    /// <summary>
    /// 체력/마나 전체 회복
    /// </summary>
    public void FullRestore()
    {
        _currentHp = MaxHealth;
        _currentMana = MaxMana;
        _isDead = false;

        OnHpChanged?.Invoke(_currentHp, MaxHealth);
        OnManaChanged?.Invoke(_currentMana, MaxMana);

        Debug.Log("[PlayerCombatStats] Full restore completed");
    }

    /// <summary>
    /// 데미지 받기
    /// </summary>
    public void TakeDamage(double damage)
    {
        if (_isDead) return;

        // 회피 체크
        if (UnityEngine.Random.value < Dodge)
        {
            Debug.Log("[PlayerCombatStats] Dodged attack!");
            return;
        }

        _currentHp -= damage;
        _currentHp = Math.Max(0, _currentHp);

        OnHpChanged?.Invoke(_currentHp, MaxHealth);

        Debug.Log($"[PlayerCombatStats] Took {damage:F1} damage. HP: {_currentHp:F0}/{MaxHealth:F0}");

        if (_currentHp <= 0 && !_isDead)
        {
            Die();
        }
    }

    /// <summary>
    /// 마나 사용
    /// </summary>
    public bool UseMana(double amount)
    {
        if (_currentMana >= amount)
        {
            _currentMana -= amount;
            OnManaChanged?.Invoke(_currentMana, MaxMana);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 치명타 판정
    /// </summary>
    public bool IsCriticalHit()
    {
        return DamageCalculator.RollCritical((float)CriticalRate);
    }

    /// <summary>
    /// 회심의 일격 판정 (치명타 확률 100% 이상일 때만)
    /// </summary>
    public bool IsBlowHit()
    {
        if (CriticalRate < 1.0) return false;
        return UnityEngine.Random.value < BlowProbability;
    }

    /// <summary>
    /// 최종 데미지 계산 (치명타/회심의 일격 적용)
    /// </summary>
    public double CalculateFinalDamage(bool isCritical)
    {
        double damage = AttackDamage;

        // 회심의 일격 체크 (치명타 확률 100% 이상일 때만)
        if (CriticalRate >= 1.0 && IsBlowHit())
        {
            damage *= BlowDamage > 0 ? BlowDamage : 1.0;
        }
        else if (isCritical)
        {
            damage *= CriticalDamage;
        }

        return damage;
    }

    private void Die()
    {
        _isDead = true;
        OnDeath?.Invoke();
        Debug.Log("[PlayerCombatStats] Player died!");
    }
}

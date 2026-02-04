using UnityEngine;
using System;
using System.Collections.Generic;
using SlayerLegend.Skill;

// 플레이어의 체력, 마나, 전투 능력치를 관리함
public class PlayerStats : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private string playerStatsId = "Player_Default";

    private PlayerStatsData _statsData;

    // 이벤트
    public event Action<float, float> OnHpChanged;
    public event Action<float, float> OnManaChanged;
    public event Action OnDeath;

    // 현재 스탯
    private float _currentHp;
    private float _currentMana;

    // 스탯 수정치 (패시브 스킬용)
    private Dictionary<object, float> _attackDamagePercentModifiers = new Dictionary<object, float>();
    private Dictionary<object, float> _attackDamageModifiers = new Dictionary<object, float>();
    private Dictionary<object, float> _maxHealthPercentModifiers = new Dictionary<object, float>();
    private Dictionary<object, float> _maxHealthModifiers = new Dictionary<object, float>();
    private Dictionary<object, float> _criticalRateModifiers = new Dictionary<object, float>();
    private Dictionary<object, float> _criticalDamageModifiers = new Dictionary<object, float>();
    private Dictionary<object, float> _goldGainPercentModifiers = new Dictionary<object, float>();
    private Dictionary<object, float> _expGainPercentModifiers = new Dictionary<object, float>();

    private bool _isDead = false;

    // IStatsProvider 구현
    public float CurrentHealth => _currentHp;
    public float MaxHealth => CalculateFinalMaxHealth();
    public float CurrentMana => _currentMana;
    public float MaxMana => _statsData?.maxMana ?? 100f;
    public float AttackDamage => CalculateFinalAttackDamage();
    public float Defense => _statsData?.defense ?? 10f;
    public float CriticalRate => CalculateFinalCritRate();
    public float CriticalDamage => CalculateFinalCritDamage();

    // 공개 프로퍼티
    public PlayerStatsData StatsData => _statsData;
    public float AttackSpeed => _statsData?.attackSpeed ?? 1f;
    public float AttackRange => _statsData?.attackRange ?? 2f;
    public float DetectionRange => _statsData?.detectionRange ?? 15f;
    public bool IsDead => _isDead;

    // 확장 스탯
    public float BlowDamage => _statsData?.blowDamage ?? 0f;
    public float BlowProbability => _statsData?.blowProbability ?? 0f;
    public float Accuracy => _statsData?.accuracy ?? 1f;
    public float Dodge => _statsData?.dodge ?? 0f;
    public float GoldGainPercent => CalculateGoldGainPercent();
    public float ExpGainPercent => CalculateExpGainPercent();

    private void Awake()
    {
        LoadStatsData();
    }

    private void LoadStatsData()
    {
        _statsData = DataManager.Instance.playerStats.Get(playerStatsId);
        
        if (_statsData == null)
        {
            Debug.LogError($"[PlayerStats] PlayerStatsData not found: {playerStatsId}");
            enabled = false;
            return;
        }

        _currentHp = _statsData.maxHp;
        _currentMana = _statsData.maxMana;
    }

    private void Update()
    {
        if (_isDead || _statsData == null) return;

        // HP 회복
        if (_currentHp < MaxHealth)
        {
            RegenerateHp(_statsData.hpRegenPerSecond * Time.deltaTime);
        }

        // 마나 회복
        if (_currentMana < MaxMana)
        {
            RegenerateMana(_statsData.manaRegenPerSecond * Time.deltaTime);
        }
    }

    public void FullRestore()
    {
        _currentHp = MaxHealth;
        _currentMana = MaxMana;
        _isDead = false;

        OnHpChanged?.Invoke(_currentHp, MaxHealth);
        OnManaChanged?.Invoke(_currentMana, MaxMana);

        Debug.Log("[PlayerStats] Full restore completed");
    }

    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        // 회피 체크
        if (UnityEngine.Random.value < Dodge)
        {
            Debug.Log("[PlayerStats] Dodged attack!");
            return;
        }

        float finalDamage = DamageCalculator.CalculateDamageWithDefense(damage, Defense);

        _currentHp -= finalDamage;
        _currentHp = Mathf.Max(0, _currentHp);

        OnHpChanged?.Invoke(_currentHp, MaxHealth);

        Debug.Log($"[PlayerStats] Took {finalDamage:F1} damage. HP: {_currentHp:F0}/{MaxHealth:F0}");

        if (_currentHp <= 0 && !_isDead)
        {
            Die();
        }
    }

    private void RegenerateHp(float amount)
    {
        _currentHp += amount;
        _currentHp = Mathf.Min(_currentHp, MaxHealth);
        OnHpChanged?.Invoke(_currentHp, MaxHealth);
    }

    private void RegenerateMana(float amount)
    {
        _currentMana += amount;
        _currentMana = Mathf.Min(_currentMana, MaxMana);
        OnManaChanged?.Invoke(_currentMana, MaxMana);
    }

    private void Die()
    {
        _isDead = true;
        OnDeath?.Invoke();
        Debug.Log("[PlayerStats] Player died!");
    }

    public bool UseMana(float amount)
    {
        if (_currentMana >= amount)
        {
            _currentMana -= amount;
            OnManaChanged?.Invoke(_currentMana, MaxMana);
            return true;
        }
        return false;
    }

    public bool IsCriticalHit()
    {
        return DamageCalculator.RollCritical(CriticalRate);
    }

    public bool IsBlowHit()
    {
        // 치명타 확률이 100% 이상일 때만 회심의 일격 가능
        if (CriticalRate < 1f) return false;
        return UnityEngine.Random.value < BlowProbability;
    }

    public float CalculateFinalDamage(bool isCritical)
    {
        float damage = AttackDamage;
        
        // 회심의 일격 체크 (치명타 확률 100% 이상일 때만)
        if (CriticalRate >= 1f && IsBlowHit())
        {
            damage *= BlowDamage > 0 ? BlowDamage : 1f;
        }
        else if (isCritical)
        {
            damage *= CriticalDamage;
        }
        
        return damage;
    }

    // 스탯 수정자 관리
    public void AddAttackDamagePercentModifier(object source, float value) => _attackDamagePercentModifiers[source] = value;
    public void AddAttackDamageModifier(object source, float value) => _attackDamageModifiers[source] = value;
    public void AddMaxHealthPercentModifier(object source, float value) => _maxHealthPercentModifiers[source] = value;
    public void AddMaxHealthModifier(object source, float value) => _maxHealthModifiers[source] = value;
    public void AddCriticalRateModifier(object source, float value) => _criticalRateModifiers[source] = value;
    public void AddCriticalDamageModifier(object source, float value) => _criticalDamageModifiers[source] = value;
    public void AddGoldGainPercentModifier(object source, float value) => _goldGainPercentModifiers[source] = value;
    public void AddExpGainPercentModifier(object source, float value) => _expGainPercentModifiers[source] = value;

    public void RemoveAttackDamagePercentModifier(object source) => _attackDamagePercentModifiers.Remove(source);
    public void RemoveAttackDamageModifier(object source) => _attackDamageModifiers.Remove(source);
    public void RemoveMaxHealthPercentModifier(object source) => _maxHealthPercentModifiers.Remove(source);
    public void RemoveMaxHealthModifier(object source) => _maxHealthModifiers.Remove(source);
    public void RemoveCriticalRateModifier(object source) => _criticalRateModifiers.Remove(source);
    public void RemoveCriticalDamageModifier(object source) => _criticalDamageModifiers.Remove(source);
    public void RemoveGoldGainPercentModifier(object source) => _goldGainPercentModifiers.Remove(source);
    public void RemoveExpGainPercentModifier(object source) => _expGainPercentModifiers.Remove(source);

    // 스탯 계산
    private float CalculateFinalAttackDamage()
    {
        if (_statsData == null) return 50f;
        
        float baseDmg = _statsData.attackDamage;
        float flat = 0f;
        float percent = 1f;

        foreach (var mod in _attackDamageModifiers.Values) flat += mod;
        foreach (var mod in _attackDamagePercentModifiers.Values) percent += mod / 100f;

        return (baseDmg + flat) * percent;
    }

    private float CalculateFinalMaxHealth()
    {
        if (_statsData == null) return 1000f;
        
        float baseHp = _statsData.maxHp;
        float flat = 0f;
        float percent = 1f;

        foreach (var mod in _maxHealthModifiers.Values) flat += mod;
        foreach (var mod in _maxHealthPercentModifiers.Values) percent += mod / 100f;

        return (baseHp + flat) * percent;
    }

    private float CalculateFinalCritRate()
    {
        if (_statsData == null) return 0.15f;
        
        float rate = _statsData.criticalRate;
        foreach (var mod in _criticalRateModifiers.Values) rate += mod / 100f;

        return Mathf.Clamp01(rate);
    }

    private float CalculateFinalCritDamage()
    {
        if (_statsData == null) return 2f;
        
        float dmg = _statsData.criticalDamage;
        foreach (var mod in _criticalDamageModifiers.Values) dmg += mod / 100f;

        return Mathf.Max(1f, dmg);
    }

    private float CalculateGoldGainPercent()
    {
        if (_statsData == null) return 0f;
        
        float percent = _statsData.goldGainPercent;
        foreach (var mod in _goldGainPercentModifiers.Values) percent += mod;

        return percent;
    }

    private float CalculateExpGainPercent()
    {
        if (_statsData == null) return 0f;
        
        float percent = _statsData.expGainPercent;
        foreach (var mod in _expGainPercentModifiers.Values) percent += mod;

        return percent;
    }
}

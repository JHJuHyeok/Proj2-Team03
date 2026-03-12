using UnityEngine;
using System.Collections.Generic;
using SlayerLegend.Equipment;  // 장비 시스템 네임스페이스

namespace SlayerLegend.Skill.Testing
{
    // 테스트용 더미 캐릭터
    // IStatsProvider, IGoldProvider, IEquippable를 구현하여 스킬/장비 시스템 테스트
    public class DummyCharacter : MonoBehaviour, IStatsProvider, IGoldProvider, IEquippable
    {
        [Header("스탯")]
        [SerializeField] private float maxHealth = 1000f;
        [SerializeField] private float maxMana = 500f;
        [SerializeField] private float attackDamage = 50f;
        [SerializeField] private float defense = 20f;
        [SerializeField] private float criticalRate = 0.2f;  // 20%
        [SerializeField] private float criticalDamage = 1.5f; // 150%

        // 재계산된 스탯 (버프 반영)
        private float _maxHealth;
        private float _criticalRate;
        private float _criticalDamage;

        [Header("회복")]
        [SerializeField] private float manaRegenPerSecond = 30f;
        [SerializeField] private float healthRegenPerSecond = 5f;

        [Header("속도")]
        [SerializeField] private float attackSpeed = 1.0f;    // 공격 속도 배율
        [SerializeField] private float moveSpeed = 1.0f;      // 이동 속도 배율

        // 재계산된 속도 스탯 (버프 반영)
        private float _attackSpeed;
        private float _moveSpeed;
        private float _manaRegen;
        private float _healthRegen;

        [Header("골드")]
        [SerializeField] private long startingGold = 10000L;

        // 현재 값
        public float CurrentHealth { get; private set; }
        public float CurrentMana { get; private set; }
        public float MaxHealth => _maxHealth;
        public float MaxMana => maxMana;
        public double AttackDamage { get; private set; }
        public float Defense => defense;
        public float CriticalRate => _criticalRate;
        public double CriticalDamage => _criticalDamage;

        //확장 스탯 프로퍼티
        public float AttackSpeed => _attackSpeed;
        public float MoveSpeed => _moveSpeed;
        public float ManaRegen => _manaRegen;
        public float HealthRegen => _healthRegen;

        public long CurrentGold { get; private set; }

        // 버프 모디파이어 (source -> value)
        private readonly Dictionary<object, float> _attackDamagePercentModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _attackDamageModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _maxHealthPercentModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _maxHealthModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _criticalRateModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _criticalDamageModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _goldGainPercentModifiers = new Dictionary<object, float>();

        //확장 버프 모디파이어
        private readonly Dictionary<object, float> _attackSpeedModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _moveSpeedModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _manaRegenModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _healthRegenModifiers = new Dictionary<object, float>();

        //Phase 8 특수 버프 모디파이어
        private readonly Dictionary<object, float> _evasionModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _cooldownReductionModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _missingHpDamageModifiers = new Dictionary<object, float>();

        public event System.Action<long> OnGoldChanged;

        private void Awake()
        {
            // 기본값 초기화
            _maxHealth = maxHealth;
            _criticalRate = criticalRate;
            _criticalDamage = criticalDamage;

            //확장 스탯 초기화
            _attackSpeed = attackSpeed;
            _moveSpeed = moveSpeed;
            _manaRegen = manaRegenPerSecond;
            _healthRegen = healthRegenPerSecond;

            CurrentHealth = _maxHealth;
            CurrentMana = maxMana;
            CurrentGold = startingGold;
            AttackDamage = attackDamage;
        }

        private void Start()
        {
        }

        private void Update()
        {
            // 마나 자연 회복 (버프 적용)
            if (CurrentMana < MaxMana)
            {
                CurrentMana = Mathf.Min(MaxMana, CurrentMana + _manaRegen * Time.deltaTime);
            }

            //체력 자연 회복 (버프 적용)
            if (CurrentHealth < MaxHealth)
            {
                CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + _healthRegen * Time.deltaTime);
            }
        }

        #region IStatsProvider 구현
        public bool UseMana(float amount)
        {
            if (CurrentMana < amount)
            {
                return false;
            }

            CurrentMana -= amount;
            return true;
        }

        public bool IsCriticalHit()
        {
            bool isCritical = Random.value <= CriticalRate;
            return isCritical;
        }

        public double CalculateFinalDamage(bool isCritical)
        {
            double damage = AttackDamage;
            if (isCritical)
                damage *= CriticalDamage;
            return damage;
        }

        // 버프 모디파이어 추가/제거
        public void AddAttackDamagePercentModifier(object source, float value)
        {
            if (_attackDamagePercentModifiers.ContainsKey(source)) return;
            _attackDamagePercentModifiers[source] = value;
            RecalculateStats();
        }

        public void AddAttackDamageModifier(object source, float value)
        {
            if (_attackDamageModifiers.ContainsKey(source)) return;
            _attackDamageModifiers[source] = value;
            RecalculateStats();
        }

        public void AddMaxHealthPercentModifier(object source, float value)
        {
            if (_maxHealthPercentModifiers.ContainsKey(source)) return;
            _maxHealthPercentModifiers[source] = value;
            RecalculateStats();
        }

        public void AddMaxHealthModifier(object source, float value)
        {
            if (_maxHealthModifiers.ContainsKey(source)) return;
            _maxHealthModifiers[source] = value;
            RecalculateStats();
        }

        public void AddCriticalRateModifier(object source, float value)
        {
            if (_criticalRateModifiers.ContainsKey(source)) return;
            _criticalRateModifiers[source] = value;
            RecalculateStats();
        }

        public void AddCriticalDamageModifier(object source, float value)
        {
            if (_criticalDamageModifiers.ContainsKey(source)) return;
            _criticalDamageModifiers[source] = value;
            RecalculateStats();
        }

        public void AddGoldGainPercentModifier(object source, float value)
        {
            if (_goldGainPercentModifiers.ContainsKey(source)) return;
            _goldGainPercentModifiers[source] = value;
        }

        //확장 버프 모디파이어 Add
        public void AddAttackSpeedModifier(object source, float value)
        {
            if (_attackSpeedModifiers.ContainsKey(source)) return;
            _attackSpeedModifiers[source] = value;
            RecalculateStats();
        }

        public void AddMoveSpeedModifier(object source, float value)
        {
            if (_moveSpeedModifiers.ContainsKey(source)) return;
            _moveSpeedModifiers[source] = value;
            RecalculateStats();
        }

        public void AddManaRegenModifier(object source, float value)
        {
            if (_manaRegenModifiers.ContainsKey(source)) return;
            _manaRegenModifiers[source] = value;
            RecalculateStats();
        }

        public void AddHealthRegenModifier(object source, float value)
        {
            if (_healthRegenModifiers.ContainsKey(source)) return;
            _healthRegenModifiers[source] = value;
            RecalculateStats();
        }

        public void RemoveAttackDamagePercentModifier(object source)
        {
            if (_attackDamagePercentModifiers.Remove(source))
                RecalculateStats();
        }

        public void RemoveAttackDamageModifier(object source)
        {
            if (_attackDamageModifiers.Remove(source))
                RecalculateStats();
        }

        public void RemoveMaxHealthPercentModifier(object source)
        {
            if (_maxHealthPercentModifiers.Remove(source))
                RecalculateStats();
        }

        public void RemoveMaxHealthModifier(object source)
        {
            if (_maxHealthModifiers.Remove(source))
                RecalculateStats();
        }

        public void RemoveCriticalRateModifier(object source)
        {
            if (_criticalRateModifiers.Remove(source))
                RecalculateStats();
        }

        public void RemoveCriticalDamageModifier(object source)
        {
            if (_criticalDamageModifiers.Remove(source))
                RecalculateStats();
        }

        public void RemoveGoldGainPercentModifier(object source)
        {
            _goldGainPercentModifiers.Remove(source);
        }

        //확장 버프 모디파이어 Remove
        public void RemoveAttackSpeedModifier(object source)
        {
            if (_attackSpeedModifiers.Remove(source))
                RecalculateStats();
        }

        public void RemoveMoveSpeedModifier(object source)
        {
            if (_moveSpeedModifiers.Remove(source))
                RecalculateStats();
        }

        public void RemoveManaRegenModifier(object source)
        {
            if (_manaRegenModifiers.Remove(source))
                RecalculateStats();
        }

        public void RemoveHealthRegenModifier(object source)
        {
            if (_healthRegenModifiers.Remove(source))
                RecalculateStats();
        }

        //Phase 8 특수 버프 모디파이어 Add
        public void AddEvasionModifier(object source, float value)
        {
            if (_evasionModifiers.ContainsKey(source)) return;
            _evasionModifiers[source] = value;
            RecalculateStats();
        }

        public void AddCooldownReductionModifier(object source, float value)
        {
            if (_cooldownReductionModifiers.ContainsKey(source)) return;
            _cooldownReductionModifiers[source] = value;
            RecalculateStats();
        }

        public void AddMissingHpDamageModifier(object source, float value)
        {
            if (_missingHpDamageModifiers.ContainsKey(source)) return;
            _missingHpDamageModifiers[source] = value;
            RecalculateStats();
        }

        //Phase 8 특수 버프 모디파이어 Remove
        public void RemoveEvasionModifier(object source)
        {
            if (_evasionModifiers.Remove(source))
                RecalculateStats();
        }

        public void RemoveCooldownReductionModifier(object source)
        {
            if (_cooldownReductionModifiers.Remove(source))
                RecalculateStats();
        }

        public void RemoveMissingHpDamageModifier(object source)
        {
            if (_missingHpDamageModifiers.Remove(source))
                RecalculateStats();
        }

        //Phase 8 체력 조작
        public void SacrificeHealth(float percent)
        {
            float sacrificeAmount = MaxHealth * percent;
            CurrentHealth = Mathf.Max(0, CurrentHealth - sacrificeAmount);
        }

        public void RestoreHealth(float percent)
        {
            float restoreAmount = MaxHealth * percent;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + restoreAmount);
        }

        public void RestoreMana(float percent)
        {
            float restoreAmount = MaxMana * percent;
            CurrentMana = Mathf.Min(MaxMana, CurrentMana + restoreAmount);
        }

        private void RecalculateStats()
        {
            // 공격력 재계산
            float baseAttack = attackDamage;
            float percentBonus = 0f;
            foreach (var mod in _attackDamagePercentModifiers.Values)
                percentBonus += mod;

            float flatBonus = 0f;
            foreach (var mod in _attackDamageModifiers.Values)
                flatBonus += mod;

            AttackDamage = baseAttack * (1 + percentBonus / 100f) + flatBonus;

            // 체력 재계산
            float baseHealth = maxHealth;
            float healthPercentBonus = 0f;
            foreach (var mod in _maxHealthPercentModifiers.Values)
                healthPercentBonus += mod;

            float healthFlatBonus = 0f;
            foreach (var mod in _maxHealthModifiers.Values)
                healthFlatBonus += mod;

            _maxHealth = baseHealth * (1 + healthPercentBonus / 100f) + healthFlatBonus;

            // 현재 체력이 최대 체력을 초과하면 조정
            if (CurrentHealth > _maxHealth)
                CurrentHealth = _maxHealth;

            // 크리율 재계산
            _criticalRate = criticalRate;
            foreach (var mod in _criticalRateModifiers.Values)
                _criticalRate += mod;

            // 크리티컬 데미지 재계산
            _criticalDamage = criticalDamage;
            foreach (var mod in _criticalDamageModifiers.Values)
                _criticalDamage += mod;

            //확장 스탯 재계산
            // 공격 속도 재계산
            _attackSpeed = attackSpeed;
            foreach (var mod in _attackSpeedModifiers.Values)
                _attackSpeed *= (1 + mod / 100f);

            // 이동 속도 재계산
            _moveSpeed = moveSpeed;
            foreach (var mod in _moveSpeedModifiers.Values)
                _moveSpeed *= (1 + mod / 100f);

            // 마나 회복 재계산
            _manaRegen = manaRegenPerSecond;
            foreach (var mod in _manaRegenModifiers.Values)
                _manaRegen *= (1 + mod / 100f);

            // 체력 회복 재계산
            _healthRegen = healthRegenPerSecond;
            foreach (var mod in _healthRegenModifiers.Values)
                _healthRegen *= (1 + mod / 100f);

            //Phase 8 특수 스탯 재계산
            // 회피율 (기본 0%)
            float evasionRate = 0f;
            foreach (var mod in _evasionModifiers.Values)
                evasionRate += mod;

            // 쿨타임 감소 (기본 0%)
            float cooldownReduction = 0f;
            foreach (var mod in _cooldownReductionModifiers.Values)
                cooldownReduction += mod;

            // 잃은 체력 비례 공격력 (기본 0%)
            float missingHpDamageBonus = 0f;
            foreach (var mod in _missingHpDamageModifiers.Values)
                missingHpDamageBonus += mod;

            // 잃은 체력 비례 공격력 보너스 적용
            if (missingHpDamageBonus > 0)
            {
                float missingHpPercent = 1f - (CurrentHealth / MaxHealth);
                double bonusDamage = AttackDamage * missingHpDamageBonus / 100.0 * missingHpPercent;
                AttackDamage += bonusDamage;
            }
        }
        #endregion

        #region IGoldProvider 구현
        public bool HasEnoughGold(long amount)
        {
            return CurrentGold >= amount;
        }

        public bool SpendGold(long amount)
        {
            if (!HasEnoughGold(amount))
            {
                return false;
            }

            CurrentGold -= amount;
            OnGoldChanged?.Invoke(CurrentGold);
            return true;
        }

        public void AddGold(long amount)
        {
            CurrentGold += amount;
            OnGoldChanged?.Invoke(CurrentGold);
        }
        #endregion

        // 테스트용 골드 추가 (GUI 테스트용)
        public void TestAddGold(long amount)
        {
            AddGold(amount);
        }

        // 테스트용 회복
        public void TestHeal()
        {
            CurrentHealth = MaxHealth;
            CurrentMana = MaxMana;
        }

        #region IEquippable 구현
        // 장비 효과 적용
        // 기존 버프 모디파이어 시스템을 활용하여 장비 효과 적용
        public void ApplyEquipmentEffect(ItemEffect effect, int level, bool equip)
        {
            if (effect == null) return;

            // 레벨에 따른 최종 값 계산
            float finalValue = effect.initValue + effect.levelUpValue * (level - 1);

            // 장비를 소스로 사용 (객체 참조)
            object equipmentSource = effect;  // 각 효과를 고유한 소스로 사용

            switch (effect.type)
            {
                case EffectType.AttackBoost:
                    // 공격력 % 증가
                    if (equip)
                        AddAttackDamagePercentModifier(equipmentSource, finalValue);
                    else
                        RemoveAttackDamagePercentModifier(equipmentSource);
                    break;

                case EffectType.CriticalDamage:
                    // 크리티컬 데미지 증가
                    if (equip)
                        AddCriticalDamageModifier(equipmentSource, finalValue);
                    else
                        RemoveCriticalDamageModifier(equipmentSource);
                    break;

                case EffectType.HealthBoost:
                    // 체력 증가
                    if (equip)
                        AddMaxHealthModifier(equipmentSource, finalValue);
                    else
                        RemoveMaxHealthModifier(equipmentSource);
                    break;

                case EffectType.GoldGain:
                    // 골드 획득 % 증가
                    if (equip)
                        AddGoldGainPercentModifier(equipmentSource, finalValue);
                    else
                        RemoveGoldGainPercentModifier(equipmentSource);
                    break;

                case EffectType.ExpGain:
                    // 경험치 획득 % 증가 (현재는 로그만)
                    break;

                case EffectType.ManaBoost:
                    // 마나 증가 (추후 구현)
                    break;

                default:
                    break;
            }
        }
        #endregion
    }
}

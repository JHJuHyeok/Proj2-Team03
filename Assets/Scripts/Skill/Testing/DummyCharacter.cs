using UnityEngine;
using System.Collections.Generic;
using SlayerLegend.Equipment;  // 장비 시스템 네임스페이스

namespace SlayerLegend.Skill.Testing
{
    /// <summary>
    /// 테스트용 더미 캐릭터
    /// IStatsProvider, IGoldProvider, IEquippable를 구현하여 스킬/장비 시스템 테스트
    /// </summary>
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

        [Header("골드")]
        [SerializeField] private long startingGold = 10000L;

        // 현재 값
        public float CurrentHealth { get; private set; }
        public float CurrentMana { get; private set; }
        public float MaxHealth => _maxHealth;
        public float MaxMana => maxMana;
        public float AttackDamage { get; private set; }
        public float Defense => defense;
        public float CriticalRate => _criticalRate;
        public float CriticalDamage => _criticalDamage;

        public long CurrentGold { get; private set; }

        // 버프 모디파이어 (source -> value)
        private readonly Dictionary<object, float> _attackDamagePercentModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _attackDamageModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _maxHealthPercentModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _maxHealthModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _criticalRateModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _criticalDamageModifiers = new Dictionary<object, float>();
        private readonly Dictionary<object, float> _goldGainPercentModifiers = new Dictionary<object, float>();

        public event System.Action<long> OnGoldChanged;

        private void Awake()
        {
            // 기본값 초기화
            _maxHealth = maxHealth;
            _criticalRate = criticalRate;
            _criticalDamage = criticalDamage;

            CurrentHealth = _maxHealth;
            CurrentMana = maxMana;
            CurrentGold = startingGold;
            AttackDamage = attackDamage;
        }

        private void Start()
        {
            Debug.Log($"[DummyCharacter] 초기화 완료 - HP: {CurrentHealth}, MP: {CurrentMana}, ATK: {AttackDamage}, Gold: {CurrentGold}");
        }

        private void Update()
        {
            // 마나 자연 회복
            if (CurrentMana < MaxMana)
            {
                CurrentMana = Mathf.Min(MaxMana, CurrentMana + manaRegenPerSecond * Time.deltaTime);
            }
        }

        #region IStatsProvider 구현
        public bool UseMana(float amount)
        {
            if (CurrentMana < amount)
            {
                Debug.Log($"마나 부족! 필요: {amount}, 현재: {CurrentMana}");
                return false;
            }

            CurrentMana -= amount;
            Debug.Log($"마나 사용: -{amount}, 남은: {CurrentMana}");
            return true;
        }

        public bool IsCriticalHit()
        {
            bool isCritical = Random.value <= CriticalRate;
            return isCritical;
        }

        public float CalculateFinalDamage(bool isCritical)
        {
            float damage = AttackDamage;
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

            Debug.Log($"[DummyCharacter] 스탯 재계산 - ATK: {AttackDamage:F1}, HP: {_maxHealth:F1}, CRIT: {_criticalRate:P2}, CRIT_DMG: {_criticalDamage:F2}");
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
                Debug.Log($"골드 부족! 필요: {amount}, 현재: {CurrentGold}");
                return false;
            }

            CurrentGold -= amount;
            OnGoldChanged?.Invoke(CurrentGold);
            Debug.Log($"골드 사용: -{amount}, 남은: {CurrentGold}");
            return true;
        }

        public void AddGold(long amount)
        {
            CurrentGold += amount;
            OnGoldChanged?.Invoke(CurrentGold);
            Debug.Log($"골드 획득: +{amount}, 현재: {CurrentGold}");
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
            Debug.Log("[DummyCharacter] 체력/마나 완전 회복");
        }

        #region IEquippable 구현
        /// <summary>
        /// 장비 효과 적용
        /// 기존 버프 모디파이어 시스템을 활용하여 장비 효과 적용
        /// </summary>
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
                    Debug.Log($"[DummyCharacter] 공격력 {(equip ? "+" : "-")}{finalValue}% = {AttackDamage:F1}");
                    break;

                case EffectType.CriticalDamage:
                    // 크리티컬 데미지 증가
                    if (equip)
                        AddCriticalDamageModifier(equipmentSource, finalValue);
                    else
                        RemoveCriticalDamageModifier(equipmentSource);
                    Debug.Log($"[DummyCharacter] 크리뎀 {(equip ? "+" : "-")}{finalValue:F2} = {CriticalDamage:F2}");
                    break;

                case EffectType.HealthBoost:
                    // 체력 증가
                    if (equip)
                        AddMaxHealthModifier(equipmentSource, finalValue);
                    else
                        RemoveMaxHealthModifier(equipmentSource);
                    Debug.Log($"[DummyCharacter] 체력 {(equip ? "+" : "-")}{finalValue} = {MaxHealth:F1}");
                    break;

                case EffectType.GoldGain:
                    // 골드 획득 % 증가
                    if (equip)
                        AddGoldGainPercentModifier(equipmentSource, finalValue);
                    else
                        RemoveGoldGainPercentModifier(equipmentSource);
                    Debug.Log($"[DummyCharacter] 골드 획득 {(equip ? "+" : "-")}{finalValue}%");
                    break;

                case EffectType.ExpGain:
                    // 경험치 획득 % 증가 (현재는 로그만)
                    Debug.Log($"[DummyCharacter] 경험치 획득 {(equip ? "+" : "-")}{finalValue}%");
                    break;

                case EffectType.ManaBoost:
                    // 마나 증가 (추후 구현)
                    Debug.Log($"[DummyCharacter] 마나 증가 {finalValue} (미구현)");
                    break;

                default:
                    Debug.LogWarning($"[DummyCharacter] 알 수 없는 장비 효과: {effect.type}");
                    break;
            }
        }
        #endregion
    }
}

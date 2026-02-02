using UnityEngine;
using System.Collections.Generic;

namespace SlayerLegend.Skill.Testing
{
    /// <summary>
    /// 테스트용 더미 캐릭터
    /// IStatsProvider와 IGoldProvider를 구현하여 스킬 시스템 테스트
    /// </summary>
    public class DummyCharacter : MonoBehaviour, IStatsProvider, IGoldProvider
    {
        [Header("스탯")]
        [SerializeField] private float maxHealth = 1000f;
        [SerializeField] private float maxMana = 500f;
        [SerializeField] private float attackDamage = 50f;
        [SerializeField] private float defense = 20f;
        [SerializeField] private float criticalRate = 0.2f;  // 20%
        [SerializeField] private float criticalDamage = 1.5f; // 150%

        [Header("회복")]
        [SerializeField] private float manaRegenPerSecond = 30f;

        [Header("골드")]
        [SerializeField] private long startingGold = 10000L;

        // 현재 값
        public float CurrentHealth { get; private set; }
        public float CurrentMana { get; private set; }
        public float MaxHealth => maxHealth;
        public float MaxMana => maxMana;
        public float AttackDamage { get; private set; }
        public float Defense => defense;
        public float CriticalRate => criticalRate;
        public float CriticalDamage => criticalDamage;

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
            CurrentHealth = maxHealth;
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

            // 크리율 재계산
            float critRate = criticalRate;
            foreach (var mod in _criticalRateModifiers.Values)
                critRate += mod;

            // 나머지 스탯도 필요시 재계산...

            Debug.Log($"[DummyCharacter] 스탯 재계산 - ATK: {AttackDamage}");
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
    }
}

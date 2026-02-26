using UnityEngine;
using System.Collections.Generic;
using SlayerLegend.Skill;

namespace SlayerLegend.Combat
{
    // PlayerCombatStats를 IStatsProvider로 변환하는 어댑터 컴포넌트
    // 팀원 파일(PlayerCombatStats.cs)을 수정하지 않고 스킬 시스템과 연동
    //
    // 작성자: 조민희
    // 작성일: 2025-02-06
    // 수정일: 2026-02-26 (버프 모디파이어 실제 구현)
    // 설명: 어댑터 패턴으로 PlayerCombatStats의 double 타입 스탯을 IStatsProvider의 float로 변환
    //       패시브 스킬의 버프 모디파이어를 내부 Dictionary로 관리하여 스탯에 반영
    public class PlayerStatsAdapter : MonoBehaviour, IStatsProvider
    {
        private PlayerCombatStats _stats;

        // 버프 모디파이어 관리 (패시브 스킬용)
        private Dictionary<object, float> _attackDamagePercentModifiers = new();
        private Dictionary<object, float> _attackDamageFlatModifiers = new();
        private Dictionary<object, float> _maxHealthPercentModifiers = new();
        private Dictionary<object, float> _maxHealthFlatModifiers = new();
        private Dictionary<object, float> _criticalRateModifiers = new();
        private Dictionary<object, float> _criticalDamageModifiers = new();
        private Dictionary<object, float> _attackSpeedModifiers = new();
        private Dictionary<object, float> _moveSpeedModifiers = new();
        private Dictionary<object, float> _manaRegenModifiers = new();
        private Dictionary<object, float> _healthRegenModifiers = new();
        private Dictionary<object, float> _evasionModifiers = new();
        private Dictionary<object, float> _cooldownReductionModifiers = new();
        private Dictionary<object, float> _missingHpDamageModifiers = new();
        private Dictionary<object, float> _goldGainModifiers = new();

        // 기본 스탯 (버프 적용 전)
        private float _baseAttackSpeed = 1f;
        private float _baseMoveSpeed = 1f;
        private float _baseEvasion = 0f;
        private float _baseCooldownReduction = 0f;

        private void Awake()
        {
            // 같은 GameObject의 PlayerCombatStats 찾기
            _stats = GetComponent<PlayerCombatStats>();

            if (_stats == null)
            {
                Debug.LogWarning($"[PlayerStatsAdapter] PlayerCombatStats 컴포넌트를 찾을 수 없습니다: {gameObject.name}");
            }
        }

        // 모디파이어 합계 계산 헬퍼
        private float SumModifiers(Dictionary<object, float> modifiers)
        {
            float sum = 0f;
            foreach (var value in modifiers.Values)
            {
                sum += value;
            }
            return sum;
        }

        // === IStatsProvider 프로퍼티 구현 (double → float 변환 + 버프 적용) ===

        public float CurrentHealth => _stats != null ? (float)_stats.CurrentHealth : 0f;
        public float MaxHealth
        {
            get
            {
                if (_stats == null) return 0f;
                float baseMax = (float)_stats.MaxHealth;
                // 퍼센트 버프 적용
                float percentBonus = baseMax * (SumModifiers(_maxHealthPercentModifiers) / 100f);
                // 고정값 버프 적용
                float flatBonus = SumModifiers(_maxHealthFlatModifiers);
                return baseMax + percentBonus + flatBonus;
            }
        }
        public float CurrentMana => _stats != null ? (float)_stats.CurrentMana : 0f;
        public float MaxMana => _stats != null ? (float)_stats.MaxMana : 0f;

        public double AttackDamage
        {
            get
            {
                if (_stats == null) return 0.0;
                double baseDamage = _stats.AttackDamage;
                // 퍼센트 버프 적용
                double percentBonus = baseDamage * (SumModifiers(_attackDamagePercentModifiers) / 100.0);
                // 고정값 버프 적용
                double flatBonus = SumModifiers(_attackDamageFlatModifiers);
                return baseDamage + percentBonus + flatBonus;
            }
        }

        public float Defense => 0f;  // 현재 방어력 스탯 없음

        public float CriticalRate
        {
            get
            {
                if (_stats == null) return 0f;
                float baseCrit = _stats != null ? (float)_stats.CriticalRate : 0f;
                return baseCrit + SumModifiers(_criticalRateModifiers);
            }
        }

        public double CriticalDamage
        {
            get
            {
                if (_stats == null) return 0.0;
                double baseCritDmg = _stats.CriticalDamage;
                return baseCritDmg + SumModifiers(_criticalDamageModifiers);
            }
        }

        // === IStatsProvider 메서드 구현 (위임) ===

        public bool UseMana(float amount)
        {
            return _stats != null && _stats.UseMana((double)amount);
        }

        public bool IsCriticalHit()
        {
            return _stats != null && _stats.IsCriticalHit();
        }

        public double CalculateFinalDamage(bool isCritical)
        {
            return _stats != null ? _stats.CalculateFinalDamage(isCritical) : 0.0;
        }

        // === 버프 모디파이어 (패시브 스킬용) - 실제 구현 ===

        public void AddAttackDamagePercentModifier(object source, float value)
        {
            _attackDamagePercentModifiers[source] = value;
        }

        public void RemoveAttackDamagePercentModifier(object source)
        {
            _attackDamagePercentModifiers.Remove(source);
        }

        public void AddAttackDamageModifier(object source, float value)
        {
            _attackDamageFlatModifiers[source] = value;
        }

        public void RemoveAttackDamageModifier(object source)
        {
            _attackDamageFlatModifiers.Remove(source);
        }

        public void AddMaxHealthPercentModifier(object source, float value)
        {
            _maxHealthPercentModifiers[source] = value;
        }

        public void RemoveMaxHealthPercentModifier(object source)
        {
            _maxHealthPercentModifiers.Remove(source);
        }

        public void AddMaxHealthModifier(object source, float value)
        {
            _maxHealthFlatModifiers[source] = value;
        }

        public void RemoveMaxHealthModifier(object source)
        {
            _maxHealthFlatModifiers.Remove(source);
        }

        public void AddCriticalRateModifier(object source, float value)
        {
            _criticalRateModifiers[source] = value;
        }

        public void RemoveCriticalRateModifier(object source)
        {
            _criticalRateModifiers.Remove(source);
        }

        public void AddCriticalDamageModifier(object source, float value)
        {
            _criticalDamageModifiers[source] = value;
        }

        public void RemoveCriticalDamageModifier(object source)
        {
            _criticalDamageModifiers.Remove(source);
        }

        public void AddGoldGainPercentModifier(object source, float value)
        {
            _goldGainModifiers[source] = value;
        }

        public void RemoveGoldGainPercentModifier(object source)
        {
            _goldGainModifiers.Remove(source);
        }

        // 조민희 추가: 확장 버프 모디파이어
        public void AddAttackSpeedModifier(object source, float value)
        {
            _attackSpeedModifiers[source] = value;
        }

        public void AddMoveSpeedModifier(object source, float value)
        {
            _moveSpeedModifiers[source] = value;
        }

        public void AddManaRegenModifier(object source, float value)
        {
            _manaRegenModifiers[source] = value;
        }

        public void AddHealthRegenModifier(object source, float value)
        {
            _healthRegenModifiers[source] = value;
        }

        public void RemoveAttackSpeedModifier(object source)
        {
            _attackSpeedModifiers.Remove(source);
        }

        public void RemoveMoveSpeedModifier(object source)
        {
            _moveSpeedModifiers.Remove(source);
        }

        public void RemoveManaRegenModifier(object source)
        {
            _manaRegenModifiers.Remove(source);
        }

        public void RemoveHealthRegenModifier(object source)
        {
            _healthRegenModifiers.Remove(source);
        }

        // 조민희 추가: Phase 8 특수 버프 모디파이어
        public void AddEvasionModifier(object source, float value)
        {
            _evasionModifiers[source] = value;
        }

        public void AddCooldownReductionModifier(object source, float value)
        {
            _cooldownReductionModifiers[source] = value;
        }

        public void AddMissingHpDamageModifier(object source, float value)
        {
            _missingHpDamageModifiers[source] = value;
        }

        public void RemoveEvasionModifier(object source)
        {
            _evasionModifiers.Remove(source);
        }

        public void RemoveCooldownReductionModifier(object source)
        {
            _cooldownReductionModifiers.Remove(source);
        }

        public void RemoveMissingHpDamageModifier(object source)
        {
            _missingHpDamageModifiers.Remove(source);
        }

        // 조민희 추가: Phase 8 체력 조작
        public void SacrificeHealth(float percent)
        {
            if (_stats == null) return;
            float sacrificeAmount = MaxHealth * percent;
            _stats.TakeDamage(sacrificeAmount);
        }

        public void RestoreHealth(float percent)
        {
            if (_stats == null) return;
            // PlayerCombatStats에는 직접 회복 메서드가 없으므로
            // 현재 HP/MP 회복은 미구현 상태로 둠 (TODO: 추후 구현)
            // float restoreAmount = MaxHealth * percent;
        }

        public void RestoreMana(float percent)
        {
            if (_stats == null) return;
            // PlayerCombatStats의 UseMana는 소모만 가능하므로
            // 마나 회복은 미구현 상태로 둠 (TODO: 추후 구현)
            // float restoreAmount = MaxMana * percent;
        }

        // === 추가 스탯 조회 프로퍼티 (확장용) ===

        // 공격속도 (버프 적용)
        public float AttackSpeed => _baseAttackSpeed + SumModifiers(_attackSpeedModifiers);

        // 이동속도 (버프 적용)
        public float MoveSpeed => _baseMoveSpeed + SumModifiers(_moveSpeedModifiers);

        // 회피율 (버프 적용)
        public float Evasion => _baseEvasion + SumModifiers(_evasionModifiers);

        // 쿨타임 감소 (버프 적용)
        public float CooldownReduction => _baseCooldownReduction + SumModifiers(_cooldownReductionModifiers);

        // 잃은 체력 비례 데미지 보너스
        public float MissingHpDamageBonus => SumModifiers(_missingHpDamageModifiers);

        // 선택사항: 외부에서 직접 PlayerCombatStats에 접근할 수 있는 프로퍼티
        public PlayerCombatStats Stats => _stats;
    }
}

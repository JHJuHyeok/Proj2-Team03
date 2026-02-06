using UnityEngine;
using SlayerLegend.Skill;

namespace SlayerLegend.Combat
{
    // PlayerCombatStats를 IStatsProvider로 변환하는 어댑터 컴포넌트
    // 팀원 파일(PlayerCombatStats.cs)을 수정하지 않고 스킬 시스템과 연동
    //
    // 작성자: 조민희
    // 작성일: 2025-02-06
    // 설명: 어댑터 패턴으로 PlayerCombatStats의 double 타입 스탯을 IStatsProvider의 float로 변환
    public class PlayerStatsAdapter : MonoBehaviour, IStatsProvider
    {
        private PlayerCombatStats _stats;

        private void Awake()
        {
            // 같은 GameObject의 PlayerCombatStats 찾기
            _stats = GetComponent<PlayerCombatStats>();

            if (_stats == null)
            {
                Debug.LogWarning($"[PlayerStatsAdapter] PlayerCombatStats 컴포넌트를 찾을 수 없습니다: {gameObject.name}");
            }
        }

        // === IStatsProvider 프로퍼티 구현 (double → float 변환) ===

        public float CurrentHealth => _stats != null ? (float)_stats.CurrentHealth : 0f;
        public float MaxHealth => _stats != null ? (float)_stats.MaxHealth : 0f;
        public float CurrentMana => _stats != null ? (float)_stats.CurrentMana : 0f;
        public float MaxMana => _stats != null ? (float)_stats.MaxMana : 0f;
        public float AttackDamage => _stats != null ? (float)_stats.AttackDamage : 0f;
        public float Defense => 0f;  // 현재 방어력 스탯 없음
        public float CriticalRate => _stats != null ? (float)_stats.CriticalRate : 0f;
        public float CriticalDamage => _stats != null ? (float)_stats.CriticalDamage : 0f;

        // === IStatsProvider 메서드 구현 (위임) ===

        public bool UseMana(float amount)
        {
            return _stats != null && _stats.UseMana((double)amount);
        }

        public bool IsCriticalHit()
        {
            return _stats != null && _stats.IsCriticalHit();
        }

        public float CalculateFinalDamage(bool isCritical)
        {
            return _stats != null ? (float)_stats.CalculateFinalDamage(isCritical) : 0f;
        }

        // === 버프 모디파이어 (패시브 스킬용) - 현재 미구현 ===
        // 데미지 스킬만 구현하므로 버프 모디파이어는 비워둠
        // TODO: 패시브 스킬 구현 시 StatController와 연동 필요

        public void AddAttackDamagePercentModifier(object source, float value)
        {
            Debug.LogWarning($"[PlayerStatsAdapter] AddAttackDamagePercentModifier: 미구현됨 (source: {source}, value: {value})");
        }

        public void RemoveAttackDamagePercentModifier(object source)
        {
            Debug.LogWarning($"[PlayerStatsAdapter] RemoveAttackDamagePercentModifier: 미구현됨 (source: {source})");
        }

        public void AddAttackDamageModifier(object source, float value)
        {
            Debug.LogWarning($"[PlayerStatsAdapter] AddAttackDamageModifier: 미구현됨 (source: {source}, value: {value})");
        }

        public void RemoveAttackDamageModifier(object source)
        {
            Debug.LogWarning($"[PlayerStatsAdapter] RemoveAttackDamageModifier: 미구현됨 (source: {source})");
        }

        public void AddMaxHealthPercentModifier(object source, float value)
        {
            Debug.LogWarning($"[PlayerStatsAdapter] AddMaxHealthPercentModifier: 미구현됨 (source: {source}, value: {value})");
        }

        public void RemoveMaxHealthPercentModifier(object source)
        {
            Debug.LogWarning($"[PlayerStatsAdapter] RemoveMaxHealthPercentModifier: 미구현됨 (source: {source})");
        }

        public void AddMaxHealthModifier(object source, float value)
        {
            Debug.LogWarning($"[PlayerStatsAdapter] AddMaxHealthModifier: 미구현됨 (source: {source}, value: {value})");
        }

        public void RemoveMaxHealthModifier(object source)
        {
            Debug.LogWarning($"[PlayerStatsAdapter] RemoveMaxHealthModifier: 미구현됨 (source: {source})");
        }

        public void AddCriticalRateModifier(object source, float value)
        {
            Debug.LogWarning($"[PlayerStatsAdapter] AddCriticalRateModifier: 미구현됨 (source: {source}, value: {value})");
        }

        public void RemoveCriticalRateModifier(object source)
        {
            Debug.LogWarning($"[PlayerStatsAdapter] RemoveCriticalRateModifier: 미구현됨 (source: {source})");
        }

        public void AddCriticalDamageModifier(object source, float value)
        {
            Debug.LogWarning($"[PlayerStatsAdapter] AddCriticalDamageModifier: 미구현됨 (source: {source}, value: {value})");
        }

        public void RemoveCriticalDamageModifier(object source)
        {
            Debug.LogWarning($"[PlayerStatsAdapter] RemoveCriticalDamageModifier: 미구현됨 (source: {source})");
        }

        public void AddGoldGainPercentModifier(object source, float value)
        {
            Debug.LogWarning($"[PlayerStatsAdapter] AddGoldGainPercentModifier: 미구현됨 (source: {source}, value: {value})");
        }

        public void RemoveGoldGainPercentModifier(object source)
        {
            Debug.LogWarning($"[PlayerStatsAdapter] RemoveGoldGainPercentModifier: 미구현됨 (source: {source})");
        }

        // 선택사항: 외부에서 직접 PlayerCombatStats에 접근할 수 있는 프로퍼티
        public PlayerCombatStats Stats => _stats;
    }
}

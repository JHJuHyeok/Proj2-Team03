using UnityEngine;

namespace SlayerLegend.Skill
{
    // 플레이어 스탯 인터페이스
    public interface IStatsProvider
    {
        // 스탯
        float CurrentHealth { get; }
        float MaxHealth { get; }
        float CurrentMana { get; }
        float MaxMana { get; }
        float AttackDamage { get; }
        float Defense { get; }
        float CriticalRate { get; }
        float CriticalDamage { get; }

        // 전투
        bool UseMana(float amount);
        bool IsCriticalHit();
        float CalculateFinalDamage(bool isCritical);

        // 버프 모디파이어 (패시브 스킬용)
        void AddAttackDamagePercentModifier(object source, float value);
        void AddAttackDamageModifier(object source, float value);
        void AddMaxHealthPercentModifier(object source, float value);
        void AddMaxHealthModifier(object source, float value);
        void AddCriticalRateModifier(object source, float value);
        void AddCriticalDamageModifier(object source, float value);
        void AddGoldGainPercentModifier(object source, float value);

        void RemoveAttackDamagePercentModifier(object source);
        void RemoveAttackDamageModifier(object source);
        void RemoveMaxHealthPercentModifier(object source);
        void RemoveMaxHealthModifier(object source);
        void RemoveCriticalRateModifier(object source);
        void RemoveCriticalDamageModifier(object source);
        void RemoveGoldGainPercentModifier(object source);
    }

    // 데미지를 받을 수 있는 대상 인터페이스
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}

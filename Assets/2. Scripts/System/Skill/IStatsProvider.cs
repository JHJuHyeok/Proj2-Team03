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
        double AttackDamage { get; }
        float Defense { get; }
        float CriticalRate { get; }
        double CriticalDamage { get; }

        // 전투
        bool UseMana(float amount);
        bool IsCriticalHit();
        double CalculateFinalDamage(bool isCritical);

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

        //확장 버프 모디파이어
        void AddAttackSpeedModifier(object source, float value);
        void AddMoveSpeedModifier(object source, float value);
        void AddManaRegenModifier(object source, float value);
        void AddHealthRegenModifier(object source, float value);

        void RemoveAttackSpeedModifier(object source);
        void RemoveMoveSpeedModifier(object source);
        void RemoveManaRegenModifier(object source);
        void RemoveHealthRegenModifier(object source);

        //Phase 8 특수 버프 모디파이어
        void AddEvasionModifier(object source, float value);
        void AddCooldownReductionModifier(object source, float value);
        void AddMissingHpDamageModifier(object source, float value);

        void RemoveEvasionModifier(object source);
        void RemoveCooldownReductionModifier(object source);
        void RemoveMissingHpDamageModifier(object source);

        //Phase 8 체력 조작
        void SacrificeHealth(float percent);  // 체력 퍼센트 소모
        void RestoreHealth(float percent);    // 체력 퍼센트 회복
        void RestoreMana(float percent);      // 마나 퍼센트 회복
    }

    // 데미지를 받을 수 있는 대상 인터페이스
    public interface IDamageable
    {
        void TakeDamage(double damage);
    }
}

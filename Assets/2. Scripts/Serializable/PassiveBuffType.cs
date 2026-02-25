using System;

namespace SlayerLegend.Skill
{
    /// <summary>
    /// 패시브 스킬 버프 타입
    /// SlayerLegend.Data 네임스페이스에서 전역으로 이동
    /// </summary>
    [Serializable]
    public enum PassiveBuffType
    {
        None,

        // 공격 관련
        AttackDamagePercent,
        AttackDamageFixed,

        // 체력 관련
        MaxHealthPercent,
        MaxHealthFixed,
        HealthRegenPercent,     // 체력 회복 %

        // 크리티컬 관련
        CriticalRate,
        CriticalDamage,

        // 공격 속도
        AttackSpeedPercent,     // 공격 속도 %

        // 이동 속도
        MoveSpeedPercent,       // 이동 속도 %

        // 마나 관련
        ManaRegenPercent,       // 마나 회복 %

        // 골드 관련
        GoldGainPercent,

        // 특수 (Phase 8 구현)
        CooldownReduction,      // 쿨타임 감소 %
        Evasion,                // 회피 확률 %
        DamageBasedOnMissingHp, // 잃은 체력 비례 공격력

        // Phase 8 추가: 특수 발동형
        HealthSacrificeAttack,  // 체력 소모 후 공격력 증가 (Fire_11)
        HealthSacrificeSpeed,   // 체력 소모 후 공속 증가 (Wind_11)
        DelayedAttackBuff,      // 지연 발동 공격력 버프 (Earth_11)
        DelayedSpeedBuff,       // 지연 발동 공속 버프 (Water_11)
        HealthManaRestore       // 체력/마나 회복 (Earth_07)
    }
}

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
        AttackDamagePercent,
        AttackDamageFixed,
        MaxHealthPercent,
        MaxHealthFixed,
        CriticalRate,
        CriticalDamage,
        GoldGainPercent
    }
}

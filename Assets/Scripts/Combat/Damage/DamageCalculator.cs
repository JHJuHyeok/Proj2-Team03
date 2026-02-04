using UnityEngine;

// 치명타와 멀티플라이어를 포함한 전투 데미지 계산 유틸리티
public static class DamageCalculator
{
    // 크리티컬을 고려한 최종 데미지 계산
    // baseDamage: 기본 데미지
    // critRate: 치명타 확률 (0.0 - 1.0)
    // critDamage: 치명타 데미지 배율 (예: 2.0 = 200%)
    // skillMultiplier: 추가 스킬 배율
    // isCritical: 출력: 치명타 발생 여부
    // 반환값: 최종 계산된 데미지
    public static float CalculateDamage(
        float baseDamage,
        float critRate,
        float critDamage,
        float skillMultiplier,
        out bool isCritical)
    {
        // 스킬 배율 적용
        float damage = baseDamage * skillMultiplier;

        // 치명타 확인
        isCritical = Random.value < critRate;

        if (isCritical)
        {
            damage *= critDamage;
        }

        return damage;
    }

    // 방어력 감소를 적용한 데미지 계산
    public static float CalculateDamageWithDefense(float damage, float defense)
    {
        // 간단한 방어 공식: damage = damage * (100 / (100 + defense))
        // 방어력이 높을수록 효율이 감소함
        float reduction = 100f / (100f + defense);
        return damage * reduction;
    }

    // 치명타 여부 빠른 확인
    public static bool RollCritical(float critRate)
    {
        return Random.value < critRate;
    }
}

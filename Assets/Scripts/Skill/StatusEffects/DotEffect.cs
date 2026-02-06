﻿using UnityEngine;

namespace SlayerLegend.Skill.StatusEffects
{
    // 도트 데미지 (Damage over Time)
    // 일정 간격으로 지속 데미지를 입히는 상태이상
    public class DotEffect : StatusEffect
    {
        [Header("도트 데미지 설정")]
        [SerializeField] private float damagePerTick = 10f;
        [SerializeField] private float tickInterval = 1f;

        private float tickTimer = 0f;
        private IDamageable target;
        private GameObject source;
        private bool isPercentageBased;  // 체력 비례 여부
        private float targetMaxHp;        // 대상 최대 체력 (비례 계산용)

        public float DamagePerTick => damagePerTick;
        public float TickInterval => tickInterval;
        public IDamageable Target => target;
        public bool IsPercentageBased => isPercentageBased;
        public float TargetMaxHp => targetMaxHp;

        // 도트 데미지 초기화
        public void Initialize(float totalDuration, float perTickDamage, float interval, IDamageable damageTarget, GameObject damageSource, bool percentageBased = false, float maxHp = 0f)
        {
            duration = totalDuration;
            damagePerTick = perTickDamage;
            tickInterval = interval;
            target = damageTarget;
            source = damageSource;
            isPercentageBased = percentageBased;
            targetMaxHp = maxHp;
            remainingTime = duration;
            isExpired = false;
            tickTimer = 0f;

            // 체력 비례인 경우 표시 변경
            if (isPercentageBased && targetMaxHp > 0)
            {
                float actualDamage = targetMaxHp * (damagePerTick / 100f);
                effectName = $"DoT ({damagePerTick:F1}%/{tickInterval}s = {actualDamage:F1})";
                Debug.Log($"[DotEffect] 적용 (체력비례) - 총 {duration}초, 최대HP의 {damagePerTick:F1}%/{tickInterval}초 간격 (실데미지: {actualDamage:F1})");
            }
            else
            {
                effectName = $"DoT ({damagePerTick:F1}/{tickInterval}s)";
                Debug.Log($"[DotEffect] 적용 (고정) - 총 {duration}초, {damagePerTick}데미지/{tickInterval}초 간격");
            }
        }

        protected override void OnTick()
        {
            if (target == null) return;

            tickTimer += Time.deltaTime;

            if (tickTimer >= tickInterval)
            {
                tickTimer = 0f;
                ApplyDotDamage();
            }
        }

        private void ApplyDotDamage()
        {
            if (target == null) return;

            float actualDamage = damagePerTick;

            // 체력 비례 데미지 계산
            if (isPercentageBased && targetMaxHp > 0)
            {
                actualDamage = targetMaxHp * (damagePerTick / 100f);
            }

            target.TakeDamage(actualDamage);
            Debug.Log($"[DotEffect] 도트 데미지 적용! {actualDamage:F1} 데미지 (남은 시간: {remainingTime:F1}s)");
        }

        protected override void OnExpire()
        {
            // 마지막 틱 적용 (tickTimer가 tickInterval의 50% 이상이면)
            if (tickTimer >= tickInterval * 0.5f)
            {
                ApplyDotDamage();
            }

            base.OnExpire();
            Debug.Log($"[DotEffect] 만료 - 총 {duration}초 동안 도트 데미지 적용 완료");
        }
    }
}

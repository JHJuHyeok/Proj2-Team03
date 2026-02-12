﻿using UnityEngine;

namespace SlayerLegend.Skill.StatusEffects
{
    // 도트 데미지 (Damage over Time)
    // 일정 간격으로 지속 데미지를 입히는 상태이상
    // 조민희 추가: 스택 시스템 (최대 5스택)
    public class DotEffect : StatusEffect
    {
        [Header("도트 데미지 설정")]
        [SerializeField] private double damagePerTick = 10.0;
        [SerializeField] private float tickInterval = 1f;
        [SerializeField] private int maxStack = 5;  // 조민희 추가: 최대 스택 수

        private float tickTimer = 0f;
        private IDamageable target;
        private GameObject source;
        private bool isPercentageBased;  // 체력 비례 여부
        private float targetMaxHp;        // 대상 최대 체력 (비례 계산용)

        // 조민희 추가: 스택 시스템 변수
        private int currentStack = 1;           // 현재 스택 수
        private string stackKey;                 // 스택 식별용 (같은 종류의 스택만 중첩)

        public double DamagePerTick => damagePerTick;
        public float TickInterval => tickInterval;
        public IDamageable Target => target;
        public bool IsPercentageBased => isPercentageBased;
        public float TargetMaxHp => targetMaxHp;
        public int CurrentStack => currentStack;  // 조민희 추가: 현재 스택 조회

        // 도트 데미지 초기화
        // 조민희 추가: stackKey 파라미터 - 같은 종류의 스택만 중첩
        public void Initialize(float totalDuration, double perTickDamage, float interval, IDamageable damageTarget, GameObject damageSource, bool percentageBased = false, float maxHp = 0f, string stackKey = "default")
        {
            duration = totalDuration;
            damagePerTick = perTickDamage;
            tickInterval = interval;
            target = damageTarget;
            source = damageSource;
            isPercentageBased = percentageBased;
            targetMaxHp = maxHp;
            this.stackKey = stackKey;
            currentStack = 1;  // 스택 초기화
            remainingTime = duration;
            isExpired = false;
            tickTimer = 0f;

            // 체력 비례인 경우 표시 변경
            if (isPercentageBased && targetMaxHp > 0)
            {
                double actualDamage = targetMaxHp * (damagePerTick / 100.0) * currentStack;
                effectName = $"DoT ({damagePerTick:F1}%x{currentStack}/{tickInterval}s = {actualDamage:F1})";
                Debug.Log($"[DotEffect] 적용 (체력비례) - 스택:{currentStack}, 총 {duration}초, 최대HP의 {damagePerTick:F1}%/{tickInterval}초 간격 (실데미지: {actualDamage:F1})");
            }
            else
            {
                double actualDamage = damagePerTick * currentStack;
                effectName = $"DoT ({damagePerTick:F1}x{currentStack}/{tickInterval}s)";
                Debug.Log($"[DotEffect] 적용 (고정) - 스택:{currentStack}, 총 {duration}초, {damagePerTick}데미지/{tickInterval}초 간격 (실데미지: {actualDamage:F1})");
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

            // 조민희 추가: 스택을 적용한 데미지 계산
            double actualDamage = damagePerTick * currentStack;

            // 체력 비례 데미지 계산
            if (isPercentageBased && targetMaxHp > 0)
            {
                actualDamage = targetMaxHp * (damagePerTick / 100.0) * currentStack;
            }

            target.TakeDamage(actualDamage);
            Debug.Log($"[DotEffect] 도트 데미지 적용! 스택:{currentStack}, {actualDamage:F1} 데미지 (남은 시간: {remainingTime:F1}s)");
        }

        protected override void OnExpire()
        {
            // 마지막 틱 적용 (tickTimer가 tickInterval의 50% 이상이면)
            if (tickTimer >= tickInterval * 0.5f)
            {
                ApplyDotDamage();
            }

            // 조민희 추가: 스택 관리 (2스택 이상이면 지속시간만 초기화)
            if (currentStack > 1)
            {
                currentStack--;
                remainingTime = duration;  // 시간 초기화
                tickTimer = 0f;
                isExpired = false;  // 만료 상태 해제
                Debug.Log($"[DotEffect] 스택 감소! {currentStack + 1} → {currentStack}, 지속시간 초기화");
            }
            else
            {
                base.OnExpire();
                Debug.Log($"[DotEffect] 만료 - 총 {duration}초 동안 도트 데미지 적용 완료");
            }
        }

        // 조민희 추가: 스택 추가 (같은 종류의 DoT 중첩)
        public void AddStack(float additionalDuration, double additionalDamage, GameObject newSource)
        {
            if (currentStack < maxStack)
            {
                currentStack++;
                remainingTime = duration;  // 시간 초기화
                Debug.Log($"[DotEffect] 스택 증가! {currentStack - 1} → {currentStack}/{maxStack}");
            }
            else
            {
                // 이미 최대 스택이면 시간만 갱신
                remainingTime = duration;
                Debug.Log($"[DotEffect] 최대 스택 도달! 시간만 갱신");
            }
        }
    }
}

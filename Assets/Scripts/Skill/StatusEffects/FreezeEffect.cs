using UnityEngine;

namespace SlayerLegend.Skill.StatusEffects
{
    /// <summary>
    /// 빙결 (Freeze) - 이동 속도 감소
    /// Water 속성 효과
    /// 2중첩 시 75% 감소
    /// </summary>
    public class FreezeEffect : StatusEffect
    {
        [Header("빙결 설정")]
        [SerializeField] private float slowPercent = 0.5f; // 50% 감소

        private IFreezable freezeTarget;
        private int stackCount = 1;

        public float SlowPercent => slowPercent;
        public int StackCount => stackCount;

        /// <summary>
        /// 빙결 효과 초기화
        /// </summary>
        public void Initialize(float freezeDuration, IFreezable target)
        {
            duration = freezeDuration;
            remainingTime = duration;
            freezeTarget = target;
            isExpired = false;

            // 중첩 증가 (먼저 증가시킨 후 계산)
            freezeTarget?.IncrementFreezeStacks();
            stackCount = target?.FreezeStacks ?? 1;

            // 중첩에 따른 감소율 계산 (1중첩: 50%, 2중첩 이상: 75%)
            slowPercent = stackCount >= 2 ? 0.75f : 0.5f;

            effectName = $"빙결 x{stackCount} ({slowPercent * 100:F0}% 감소)";

            // 빙결 상태 적용 (slowPercent만 적용)
            freezeTarget?.ApplyFreeze(slowPercent);

            Debug.Log($"[FreezeEffect] 빙결 적용! {duration}초간 이속 {slowPercent * 100:F0}% 감소 (중첩: {stackCount})");
        }

        protected override void OnTick()
        {
            // 빙결 지속 중
        }

        protected override void OnExpire()
        {
            // 빙결 상태 해제
            freezeTarget?.RemoveFreeze(slowPercent);
            // DecrementFreezeStacks는 EndEffect에서만 호출 (중복 호출 방지)
            base.OnExpire();
            Debug.Log($"[FreezeEffect] 빙결 해제");
        }

        /// <summary>
        /// 빙결 효과 강제 종료
        /// </summary>
        public override void EndEffect()
        {
            freezeTarget?.RemoveFreeze(slowPercent);
            freezeTarget?.DecrementFreezeStacks();
            base.EndEffect();
        }
    }

    /// <summary>
    /// 빙결 상태가 될 수 있는 대상 인터페이스
    /// </summary>
    public interface IFreezable
    {
        /// <summary>
        /// 빙결 상태 적용 (이속 감소)
        /// </summary>
        /// <param name="slowPercent">감소할 속도 비율 (0.5 = 50% 감소)</param>
        void ApplyFreeze(float slowPercent);

        /// <summary>
        /// 빙결 상태 해제
        /// </summary>
        /// <param name="slowPercent">복구할 속도 비율</param>
        void RemoveFreeze(float slowPercent);

        /// <summary>
        /// 빙결 중첩 증가 (FreezeEffect에서 직접 호출)
        /// </summary>
        void IncrementFreezeStacks();

        /// <summary>
        /// 빙결 중첩 감소 (FreezeEffect에서 직접 호출)
        /// </summary>
        void DecrementFreezeStacks();

        /// <summary>
        /// 현재 빙결 중첩 수
        /// </summary>
        int FreezeStacks { get; }
    }
}

using UnityEngine;

namespace SlayerLegend.Skill.StatusEffects
{
    // 기절 (Stun) - 모든 행동 불가
    // CC (Crowd Control) - 속성 무관
    public class StunEffect : StatusEffect
    {
        private IStunnable stunTarget;

        public bool IsStunned => remainingTime > 0;

        // 기절 효과 초기화
        public void Initialize(float stunDuration, IStunnable target)
        {
            duration = stunDuration;
            remainingTime = duration;
            stunTarget = target;
            isExpired = false;
            effectName = $"기절 ({duration}s)";

            // 기절 상태 적용
            stunTarget?.ApplyStun(true);

            Debug.Log($"[StunEffect] 기절 적용! {duration}초간 행동 불가");
        }

        protected override void OnTick()
        {
            // 기절 지속 중
        }

        protected override void OnExpire()
        {
            // 기절 상태 해제
            stunTarget?.ApplyStun(false);
            base.OnExpire();
            Debug.Log($"[StunEffect] 기절 해제");
        }

        // 기절 효과 강제 종료
        public override void EndEffect()
        {
            stunTarget?.ApplyStun(false);
            base.EndEffect();
        }
    }

    // 기절 상태가 될 수 있는 대상 인터페이스
    public interface IStunnable
    {
        // 기절 상태 적용/해제
        void ApplyStun(bool isStunned);

        // 현재 기절 상태인지 확인
        bool IsStunned { get; }
    }
}

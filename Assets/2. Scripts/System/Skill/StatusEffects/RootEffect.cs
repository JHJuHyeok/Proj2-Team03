using UnityEngine;

namespace SlayerLegend.Skill.StatusEffects
{
    // 속박 (Root) - 이동 불가 (제자리에서만 공격 가능)
    // CC (Crowd Control) - 속성 무관
    public class RootEffect : StatusEffect
    {
        private IRootable rootTarget;

        public bool IsRooted => remainingTime > 0;

        // 속박 효과 초기화
        public void Initialize(float rootDuration, IRootable target)
        {
            duration = rootDuration;
            remainingTime = duration;
            rootTarget = target;
            isExpired = false;
            effectName = $"속박 ({duration}s)";

            // 속박 상태 적용
            rootTarget?.ApplyRoot(true);

            Debug.Log($"[RootEffect] 속박 적용! {duration}초간 이동 불가");
        }

        protected override void OnTick()
        {
            // 속박 지속 중
        }

        protected override void OnExpire()
        {
            // 속박 상태 해제
            rootTarget?.ApplyRoot(false);
            base.OnExpire();
            Debug.Log($"[RootEffect] 속박 해제");
        }

        // 속박 효과 강제 종료
        public override void EndEffect()
        {
            rootTarget?.ApplyRoot(false);
            base.EndEffect();
        }
    }

    // 속박 상태가 될 수 있는 대상 인터페이스
    public interface IRootable
    {
        // 속박 상태 적용/해제
        void ApplyRoot(bool isRooted);

        // 현재 속박 상태인지 확인
        bool IsRooted { get; }
    }
}

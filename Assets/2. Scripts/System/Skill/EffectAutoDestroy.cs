using UnityEngine;

/// <summary>
/// 이펙트 자동 삭제 컴포넌트
/// - 애니메이션 길이에 맞춰 재생 속도 자동 조정
/// - 지정된 시간 후 GameObject 삭제
/// </summary>
[RequireComponent(typeof(Animator))]
public class EffectAutoDestroy : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private float targetDuration = 1f;
    [SerializeField] private bool useAnimationLength = true;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        AdjustAnimationSpeed();
        Destroy(gameObject, targetDuration);
    }

    /// <summary>
    /// 애니메이션 재생 속도를 타겟 시간에 맞춰 조정
    /// </summary>
    private void AdjustAnimationSpeed()
    {
        if (animator == null) return;

        // 현재 재생 중인 애니메이션 클립 길이 가져오기
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float clipLength = stateInfo.length;

        // useAnimationLength가 true면 애니메이션 길이를 자동으로 사용
        if (useAnimationLength && clipLength > 0)
        {
            targetDuration = clipLength;
        }

        if (clipLength > 0 && targetDuration > 0)
        {
            // 속도 = 원래 길이 / 목표 시간
            // 예: 원래 2초 → 목표 1초 = 2배속
            // 예: 원래 2초 → 목표 3초 = 0.67배속
            float speedMultiplier = clipLength / targetDuration;
            animator.speed = speedMultiplier;
        }
    }

    /// <summary>
    /// 런타임에 타겟 시간 변경 (필요시 사용)
    /// </summary>
    public void SetDuration(float duration)
    {
        targetDuration = duration;
        AdjustAnimationSpeed();
    }
}

using UnityEngine;

/// <summary>
/// 일정 시간 후 GameObject를 자동으로 삭제하는 컴포넌트
/// 이펙트 프리팹에 사용
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("삭제까지의 대기 시간 (초)")]
    [SerializeField] private float lifetime = 1.5f;

    [Tooltip("파티클 시스템이 있으면 자동으로 재생 시간에 맞춤")]
    [SerializeField] private bool useParticleDuration = true;

    private void Start()
    {
        // 파티클 시스템이 있고 useParticleDuration이 true면 자동 계산
        if (useParticleDuration)
        {
            ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
            if (particles.Length > 0)
            {
                float maxDuration = 0f;
                foreach (var ps in particles)
                {
                    if (ps.main.duration + ps.main.startLifetime.constantMax > maxDuration)
                    {
                        maxDuration = ps.main.duration + ps.main.startLifetime.constantMax;
                    }
                }
                lifetime = Mathf.Max(lifetime, maxDuration);
            }
        }

        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// 외부에서 수명 설정
    /// </summary>
    public void SetLifetime(float time)
    {
        lifetime = time;
    }
}

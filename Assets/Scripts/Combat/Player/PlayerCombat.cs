using UnityEngine;

// 플레이어의 자동 근접 전투를 처리함
// 가장 가까운 적을 타켓팅하고 쿨다운에 맞춰 공격함
public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerTargeting playerTargeting;

    [Header("Settings")]
    [SerializeField] private bool autoAttackEnabled = true;

    // 현재 상태
    private MonsterBase _currentTarget;
    private float _lastAttackTime = -999f;

    private void Awake()
    {
        // 할당되지 않은 경우 컴포넌트 자동 찾기
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (playerTargeting == null)
            playerTargeting = GetComponent<PlayerTargeting>();

        if (playerStats == null || playerTargeting == null)
        {
            Debug.LogError("[PlayerCombat] Missing required components!");
            enabled = false;
        }
    }

    private void Update()
    {
        if (!autoAttackEnabled || playerStats.IsDead)
            return;

        // 현재 타겟이 유효하지 않으면 타겟 갱신
        if (!IsTargetValid())
        {
            // 1. 큐 우선 확인
            if (CombatManager.Instance != null && CombatManager.Instance.SpawnManager != null)
            {
                _currentTarget = CombatManager.Instance.SpawnManager.GetFirstEnemy();
            }

            // 2. 센서로 대체
            if (_currentTarget == null)
            {
                _currentTarget = playerTargeting.FindClosestEnemy();
            }

            if (_currentTarget != null)
            {
                 Debug.Log($"[PlayerCombat] 타겟 획득: {_currentTarget.name}");
            }
        }

        // 현재 타겟 공격 시도
        if (_currentTarget != null)
        {
            TryAttack();
        }
    }

    /// <summary>
    /// 범위 내에 있고 쿨다운이 지났으면 공격 시도
    /// </summary>
    private void TryAttack()
    {
        // 타겟이 공격 범위 내에 있는지 확인
        float distance = Vector3.Distance(transform.position, _currentTarget.transform.position);

        if (distance > playerStats.AttackRange)
        {
            // Debug.Log($"[PlayerCombat] Target out of range. Dist: {distance:F1}, Range: {playerStats.AttackRange}");
            return;
        }

        // 공격 쿨다운 확인
        float cooldown = 1f / playerStats.AttackSpeed; // Convert attacks/sec to cooldown

        if (Time.time >= _lastAttackTime + cooldown)
        {
            ExecuteAttack();
            _lastAttackTime = Time.time;
        }
        else
        {
            // Debug.Log($"[PlayerCombat] Attack on cooldown. Rem: {_lastAttackTime + cooldown - Time.time:F2}s");
        }
    }

    /// <summary>
    /// 현재 타겟에게 근접 공격 실행
    /// </summary>
    private void ExecuteAttack()
    {
        if (_currentTarget == null) return;

        // 치명타를 포함한 데미지 계산
        bool isCritical = DamageCalculator.RollCritical(playerStats.CriticalRate);

        float damage = DamageCalculator.CalculateDamage(
            baseDamage: playerStats.AttackDamage,
            critRate: playerStats.CriticalRate,
            critDamage: playerStats.CriticalDamage,
            skillMultiplier: 1f, // Basic attack has 1x multiplier
            out isCritical
        );

        // 타겟에게 데미지 적용
        _currentTarget.TakeDamage(damage);

        // 공격 로그
        string critText = isCritical ? " [치명타]" : "";
        Debug.Log($"[PlayerCombat] {_currentTarget.gameObject.name}에게 {damage:F1} 데미지 공격{critText}");

        // 선택사항: 공격 애니메이션/이펙트 트리거
        OnAttackExecuted(isCritical);
    }

    /// <summary>
    /// 현재 타겟이 여전히 유효한지 확인
    /// </summary>
    private bool IsTargetValid()
    {
        return playerTargeting.IsTargetValid(_currentTarget, playerStats.DetectionRange);
    }

    /// <summary>
    /// 공격 시각 효과를 위한 재정의 지점
    /// </summary>
    protected virtual void OnAttackExecuted(bool wasCritical)
    {
        // 파생 클래스에서 VFX, 애니메이션 등을 추가하기 위해 재정의
        // 예: 근접 베기 이팩트 실행, 데미지 숫자 스폰 등
    }

    /// <summary>
    /// 자동 공격 활성화/비활성화 (UI 토글용)
    /// </summary>
    public void SetAutoAttackEnabled(bool enabled)
    {
        autoAttackEnabled = enabled;
    }

    /// <summary>
    /// 현재 타겟 강제 초기화
    /// </summary>
    public void ClearTarget()
    {
        _currentTarget = null;
    }

    // 디버그 시각화
    private void OnDrawGizmosSelected()
    {
        if (playerStats == null) return;

        // 공격 범위 그리기
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerStats.AttackRange);

        // 현재 타겟까지 선 그리기
        if (_currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _currentTarget.transform.position);
        }
    }
}

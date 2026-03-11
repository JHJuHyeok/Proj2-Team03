using UnityEngine;
using SlayerLegend.Skill;  // 조민희 추가: PassiveSkill OnAttack() 연동용

// 플레이어의 자동 근접 전투를 처리함
// 가장 가까운 적을 타켓팅하고 쿨다운에 맞춰 공격함
public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombatStats playerStats;
    [SerializeField] private SkillController skillController;  // 조민희 추가: 패시브 스킬 연동

    [Header("Settings")]
    [SerializeField] private bool autoAttackEnabled = true;

    // 애니메이션
    private Animator _animator;
    private int _attackIndex = 0;

    // 현재 상태
    private MonsterBase _currentTarget;
    private float _lastAttackTime = -999f;

    private void Awake()
    {
        // 할당되지 않은 경우 컴포넌트 자동 찾기
        if (playerStats == null)
            playerStats = GetComponent<PlayerCombatStats>();

        _animator = GetComponentInChildren<Animator>();

        if (playerStats == null)
        {
            enabled = false;
        }
    }

    private void OnEnable()
    {
        // 스테이지 변경 시 타겟 초기화 이벤트 구독
        if (CombatManager.Instance != null && CombatManager.Instance.StageManager != null)
            CombatManager.Instance.StageManager.OnStageChanged += OnStageChanged;
    }

    private void OnDisable()
    {
        if (CombatManager.Instance != null && CombatManager.Instance.StageManager != null)
            CombatManager.Instance.StageManager.OnStageChanged -= OnStageChanged;
    }

    /// <summary>
    /// 스테이지 변경 시 호출 - 기존 타겟 초기화
    /// </summary>
    private void OnStageChanged(int stageIndex)
    {
        ClearTarget();
    }

    private void Update()
    {
        if (!autoAttackEnabled || playerStats.IsDead)
            return;

        // 매 프레임 맨 앞의 적을 타겟으로 갱신 (풀 재활용 시 뒤로 간 적을 계속 쫓는 문제 방지)
        if (CombatManager.Instance != null && CombatManager.Instance.SpawnManager != null)
        {
            _currentTarget = CombatManager.Instance.SpawnManager.GetFirstEnemy();
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
            return; // 공격 범위 밖이면 공격하지 않음


        // 공격 쿨다운 확인
        float cooldown = 1f / playerStats.AttackSpeed; // Convert attacks/sec to cooldown

        if (Time.time >= _lastAttackTime + cooldown)
        {
            ExecuteAttack();
            _lastAttackTime = Time.time;
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

        double damage = DamageCalculator.CalculateDamage(
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
        // Debug.Log($"[PlayerCombat] {_currentTarget.gameObject.name}에게 {damage:F1} 데미지 공격{critText}");

        // 조민희 추가: 패시브 스킬에 공격 알림 (누적형 버프용)
        NotifyPassiveSkillsOnAttack();

        // 선택사항: 공격 애니메이션/이펙트 트리거
        OnAttackExecuted(isCritical);
    }

    /// <summary>
    /// 조민희 추가: 스킬에 공격 알림 (공격 기반 누적 버프 + AttackCount 발동용)
    /// </summary>
    private void NotifyPassiveSkillsOnAttack()
    {
        if (skillController == null) return;

        // SkillController를 통해 모든 스킬에 알림
        skillController.NotifyAttackToAllSkills();
    }

    /// <summary>
    /// 현재 타겟이 여전히 유효한지 확인
    /// </summary>
    private bool IsTargetValid()
    {
        if (_currentTarget == null || _currentTarget.gameObject == null || !_currentTarget.gameObject.activeSelf)
            return false;

        float distance = Vector3.Distance(transform.position, _currentTarget.transform.position);
        return distance <= playerStats.DetectionRange;
    }

    /// <summary>
    /// 공격 시각 효과를 위한 재정의 지점
    /// </summary>
    protected virtual void OnAttackExecuted(bool wasCritical)
    {
        // 공격 애니메이션 재생 (Attack1 ↔ Attack2 번갈아 재생)
        if (_animator != null)
        {
            _animator.SetInteger("AttackIndex", _attackIndex);
            _animator.SetTrigger("Attack");

            // 0 → 1 → 0 → 1 번갈아가기
            _attackIndex = (_attackIndex + 1) % 2;
        }
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

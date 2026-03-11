using UnityEngine;

// [주혁] - MonsterData 리팩토링으로 인해 오류 코드 주석화(49. 50)

// 보스 몬스터 - 플레이어를 공격함
public class BossMonster : MonsterBase
{
    private float _lastAttackTime = -999f;
    private float _attackRange = 4f; // 공격 범위
    private double _attackPower;
    private bool _isAttacking = false;

    public override bool IsBoss => true;
    public override bool IsRewardBox => false;

    public override void Initialize(MonsterData data, StageData stageData, Transform target)
    {
        _data = data;
        _stageData = stageData;
        _target = target;
        _isDead = false;

        // 보스 HP/공격력 초기화 (StageData에서 가져옴)
        _maxHp = stageData.bossHp;
        _currentHp = _maxHp;
        _attackPower = stageData.bossAtk;

        // SpriteRenderer 캐싱
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();

        // Animator 캐싱
        if (_animator == null)
            _animator = GetComponent<Animator>();

        // Sprite Load (base의 LoadSprite + PreloadSprites 호출)
        if (!string.IsNullOrEmpty(data.spriteName))
        {
            LoadSprite(data.spriteName);
            PreloadSprites(data.spriteName);
        }

        // 기본 상태: Idle
        SetAnimState(ANIM_STATE_IDLE);
    }
    protected override void Update()
    {
        base.Update();

        if (!_isDead)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (_target == null) return;

        float distance = Vector3.Distance(transform.position, _target.position);

        // 공격 범위 안에 있는지 확인
        if (distance <= _attackRange)
        {
            // 공격 쿨다운 확인
            float cooldown = 1f; // 기본 쿨다운 값
            if (Time.time >= _lastAttackTime + cooldown)
            {
                ExecuteAttack();
                _lastAttackTime = Time.time;
            }
        }
    }

    private void ExecuteAttack()
    {
        // 공격 애니메이션 실행
        _isAttacking = true;
        SetAnimState(ANIM_STATE_ATTACK);

        // 타겟의 PlayerCombatStats 컴포넌트 찾기
        var playerStats = _target.GetComponent<PlayerCombatStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(_attackPower);
            Debug.Log($"[{_data.name}] 플레이어에게 {_attackPower:F1} 데미지 공격");
        }
        else
        {
            Debug.LogWarning($"[{_data.name}] 공격 불가: 플레이어에게 PlayerCombatStats 컴포넌트 없음");
        }

    }

    /// <summary>
    /// 공격 애니메이션 종료 시 Animation Event에서 호출
    /// </summary>
    public void OnAttackAnimationEnd()
    {
        _isAttacking = false;
        SetAnimState(ANIM_STATE_IDLE);
    }

    public override void TakeDamage(double damage)
    {
        if (_isDead) return;

        // 데미지는 적용하되, 공격 중이면 Hit 애니메이션 생략
        if (_isAttacking)
        {
            _currentHp -= damage;
            _currentHp = System.Math.Max(0, _currentHp);

            // 데미지 폰트 표시
            var prefab = CombatManager.Instance?.DamageNumberPrefab;
            if (prefab != null)
                prefab.Spawn(transform.position, damage.ToString("N0"));

            // StageUI HP 게이지 업데이트
            CombatManager.Instance?.UpdateBossHpRatio((float)HPRatio);

            if (_currentHp <= 0)
                Die();

            return;
        }

        base.TakeDamage(damage);
    }
}

using UnityEngine;

// 보스 몬스터 - 플레이어를 공격함
public class BossMonster : MonsterBase
{
    private float _lastAttackTime = -999f;
    private float _attackRange = 4f; // 공격 범위

    public override bool IsBoss => true;
    public override bool IsRewardBox => false;

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
        // 타겟의 PlayerCombatStats 컴포넌트 찾기
        var playerStats = _target.GetComponent<PlayerCombatStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage((double)_data.Attack);
            Debug.Log($"[{_data.name}] 플레이어에게 {(float)_data.Attack:F1} 데미지 공격");
        }
        else
        {
            Debug.LogWarning($"[{_data.name}] 공격 불가: 플레이어에게 PlayerCombatStats 컴포넌트 없음");
        }

        // 선택사항: 여기서 공격 애니메이션/이펙트 재생
    }
}

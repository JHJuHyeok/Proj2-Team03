using UnityEngine;

namespace SlayerLegend.Skill
{
    // 2D 스킬 발사체 (팀원 ProjectileBase 상속)
    // - 회전 없이 타겟 방향으로 이동
    public class SkillProjectile : ProjectileBase
    {
        private Vector3 _moveDirection;

        public override void Initialize(MonsterBase target, float damage, bool isCritical)
        {
            // 기본 초기화
            _target = target;
            _damage = damage;
            _isCritical = isCritical;
            _spawnTime = Time.time;
            _isActive = true;

            // 회전 초기화 (항상 0 유지)
            transform.rotation = Quaternion.identity;

            // 이동 방향 계산 (회전 없이)
            if (_target != null)
            {
                Vector3 direction = (_target.transform.position - transform.position).normalized;
                _moveDirection = direction;
            }
            else
            {
                _moveDirection = Vector3.forward;
            }
        }

        protected override void Update()
        {
            if (!_isActive) return;

            // 수명 확인
            if (Time.time >= _spawnTime + lifetime)
            {
                ReturnToPool();
                return;
            }

            // 회전 없이 이동 방향으로 직진
            transform.position += _moveDirection * moveSpeed * Time.deltaTime;
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

namespace SlayerLegend.Skill
{
    // 얼음 창: 적을 관통하는 투사체
    // - 적과 충돌해도 파괴되지 않음
    // - 같은 적은 한 번만 타격 (무한 타격 방지)
    // - 수명이 다하면 파괴됨
    public class IceSpearProjectile : SkillProjectile2D
    {
        private HashSet<Collider2D> _hitEnemies = new HashSet<Collider2D>();

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isActive) return;

            // 이미 타격한 적은 무시
            if (_hitEnemies.Contains(other))
                return;

            MonsterBase monster = other.GetComponent<MonsterBase>();
            if (monster != null && monster.gameObject.activeSelf)
            {
                OnHitEnemy(monster);
                _hitEnemies.Add(other);  // 타격한 적 기록

                // 파괴하지 않고 계속 전진
            }
        }

        protected override void DestroyProjectile()
        {
            _isActive = false;
            _hitEnemies.Clear();
            Destroy(gameObject);
        }
    }
}

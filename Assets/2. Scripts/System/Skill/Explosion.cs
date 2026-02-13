using UnityEngine;
using SlayerLegend.Skill.StatusEffects;
using System.Collections.Generic;

namespace SlayerLegend.Skill
{
    // 조민희 추가: 폭발 스킬용 컴포넌트
    // - 생성된 위치에서 일정 시간 대기 후 폭발
    // - 범위 내 모든 적에게 즉시 데미지 + DoT 적용
    public class Explosion : MonoBehaviour
    {
        private float _radius;
        private float _delay;
        private GameObject _effectPrefab;
        private double _damage;
        private bool _isCritical;
        private Vector3 _direction;
        private float _dotDuration;
        private double _dotDamage;
        private float _dotTick;
        private string _dotType;
        private Transform _caster;

        private float _spawnTime;
        private bool _hasExploded;
        private HashSet<Collider2D> _hitEnemies = new HashSet<Collider2D>();

        public void Initialize(float radius, float delay, GameObject effectPrefab,
            double damage, bool isCritical, Vector3 direction,
            float dotDuration, double dotDamage, float dotTick, string dotType,
            Transform caster)
        {
            _radius = radius;
            _delay = delay;
            _effectPrefab = effectPrefab;
            _damage = damage;
            _isCritical = isCritical;
            _direction = direction;
            _dotDuration = dotDuration;
            _dotDamage = dotDamage;
            _dotTick = dotTick;
            _dotType = dotType;
            _caster = caster;

            _spawnTime = Time.time;
        }

        private void Update()
        {
            if (_hasExploded) return;

            // 대기 시간 경과 후 폭발
            if (Time.time >= _spawnTime + _delay)
            {
                Explode();
            }
        }

        private void Explode()
        {
            _hasExploded = true;

            // 폭발 이펙트 생성
            if (_effectPrefab != null)
            {
                GameObject effect = Instantiate(_effectPrefab, transform.position, Quaternion.identity);

                // Animator가 있으면 루프 설정
                var animator = effect.GetComponent<Animator>();
                if (animator != null)
                {
                    // 애니메이션을 계속 반복 재생
                    var clipInfos = animator.GetCurrentAnimatorClipInfo(0);
                    if (clipInfos.Length > 0)
                    {
                        animator.Play(clipInfos[0].clip.name, 0, 0f);
                    }
                }

                Destroy(effect, 5f);  // 이펙트 지속 시간 5초
            }

            // 범위 내 모든 적 탐색
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _radius);

            foreach (var collider in colliders)
            {
                MonsterBase enemy = collider.GetComponent<MonsterBase>();
                if (enemy != null && enemy.gameObject.activeSelf)
                {
                    // 이미 맞은 적은 재타격 방지
                    if (!_hitEnemies.Contains(collider))
                    {
                        _hitEnemies.Add(collider);

                        // 1. 즉시 데미지
                        enemy.TakeDamage(_damage);

                        // 2. DoT 적용
                        ApplyDot(enemy);

                        string critText = _isCritical ? " [치명타]" : "";
                        Debug.Log($"[Explosion] {enemy.gameObject.name} 폭발 타격: {_damage:F1} 데미지 + {_dotType} DoT{critText}");
                    }
                }
            }

            // 오브젝트 파괴
            Destroy(gameObject);
        }

        private void ApplyDot(MonsterBase enemy)
        {
            // 기존 DoT 확인
            DotEffect existingDot = enemy.GetComponent<DotEffect>();

            if (existingDot != null)
            {
                // 있으면 스택 추가
                existingDot.AddStack(_dotDuration, _dotDamage, _caster?.gameObject);
            }
            else
            {
                // 없으면 새로 추가
                var damageable = enemy.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    var dotEffect = enemy.gameObject.AddComponent<DotEffect>();

                    dotEffect.Initialize(
                        _dotDuration,
                        _dotDamage,
                        _dotTick,
                        damageable,
                        _caster?.gameObject,
                        false,  // 화상은 체력 비례 아님
                        0f,
                        _dotType
                    );

                    Debug.Log($"[Explosion] {enemy.name}에 {_dotType} DoT 적용! {_dotDuration}초간 {_dotDamage}/{_dotTick}s마다");
                }
            }
        }

        // 폭발 범위 가시화 (디버그용)
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}

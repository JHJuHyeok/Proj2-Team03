using UnityEngine;

namespace SlayerLegend.Skill
{
    // 2D 스킬 발사체 (Physics 2D 사용)
    // - 적은 항상 오른쪽에서 오므로 오른쪽으로만 발사
    // - 적과 충돌하면 자동으로 데미지 적용
    public class SkillProjectile2D : MonoBehaviour
    {
        [Header("이동 설정")]
        [SerializeField] protected float moveSpeed = 10f;
        [SerializeField] protected float lifetime = 5f;

        [Header("시각 효과")]
        [SerializeField] protected GameObject hitEffectPrefab;

        // 런타임 데이터
        protected float _damage;
        protected bool _isCritical;
        protected float _spawnTime;
        protected bool _isActive;
        protected Vector3 _moveDirection = Vector3.right;  // 이동 방향

        // 초기화 (방향 지정 가능)
        public virtual void Initialize(Vector3 spawnPosition, float damage, bool isCritical, Vector3 direction = default)
        {
            transform.position = spawnPosition;
            _damage = damage;
            _isCritical = isCritical;
            _spawnTime = Time.time;
            _isActive = true;

            // 방향 설정 (기본값: 오른쪽)
            _moveDirection = direction == Vector3.zero ? Vector3.right : direction;

            // 회전 초기화
            transform.rotation = Quaternion.identity;
        }

        protected virtual void Update()
        {
            if (!_isActive) return;

            // 수명 확인
            if (Time.time >= _spawnTime + lifetime)
            {
                DestroyProjectile();
                return;
            }

            // 지정된 방향으로 직선 이동
            transform.position += _moveDirection * moveSpeed * Time.deltaTime;
        }

        // 2D 충돌 감지
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isActive) return;

            // 적 확인
            MonsterBase monster = other.GetComponent<MonsterBase>();
            if (monster != null && monster.gameObject.activeSelf)
            {
                OnHitEnemy(monster);
            }
        }

        // 적에게 맞았을 때
        protected virtual void OnHitEnemy(MonsterBase enemy)
        {
            // 데미지 적용
            enemy.TakeDamage(_damage);

            string critText = _isCritical ? " [치명타]" : "";
            Debug.Log($"[Projectile2D] {enemy.gameObject.name} 타격: {_damage:F1} 데미지{critText}");

            // 타격 이펙트
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // 발사체 파괴
            DestroyProjectile();
        }

        // 발사체 파괴
        protected virtual void DestroyProjectile()
        {
            _isActive = false;
            Destroy(gameObject);
        }

        // 생성 헬퍼 (방향 지정 가능)
        public static SkillProjectile2D Create(SkillProjectile2D prefab, Vector3 position, float damage, bool isCritical, Vector3 direction = default)
        {
            if (prefab == null) return null;

            SkillProjectile2D projectile = Instantiate(prefab, position, Quaternion.identity);
            projectile.Initialize(position, damage, isCritical, direction);
            return projectile;
        }
    }
}

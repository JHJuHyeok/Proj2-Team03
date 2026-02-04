using UnityEngine;

// 스킬에 의해 소환되는 투사체의 기본 클래스
// 효율적인 생성/제거를 위해 PoolManager를 사용함
[RequireComponent(typeof(Rigidbody))]
public class ProjectileBase : MonoBehaviour
{
    [Header("투사체 설정")]
    [SerializeField] protected float moveSpeed = 10f;
    [SerializeField] protected float lifetime = 5f;
    [SerializeField] protected bool useHoming = false;
    [SerializeField] protected float homingStrength = 5f;

    [Header("데미지")]
    [SerializeField] protected float baseDamage = 50f;
    [SerializeField] protected bool canCrit = true;

    [Header("시각 효과")]
    [SerializeField] protected GameObject hitEffectPrefab;

    // 런타임 데이터
    protected MonsterBase _target;
    protected float _damage;
    protected bool _isCritical;
    protected float _spawnTime;
    protected bool _isActive;

    protected Rigidbody _rigidbody;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false; // 투사체는 보통 중력을 사용하지 않음
    }

    // 타겟과 데미지 정보로 투사체 초기화
    public virtual void Initialize(MonsterBase target, float damage, bool isCritical)
    {
        _target = target;
        _damage = damage;
        _isCritical = isCritical;
        _spawnTime = Time.time;
        _isActive = true;

        // 초기에 타겟 방항을 바라봄
        if (_target != null)
        {
            Vector3 direction = (_target.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    protected virtual void Update()
    {
        if (!_isActive) return;

        // 수명 확인
        if (Time.time >= _spawnTime + lifetime)
        {
            ReturnToPool();
            return;
        }

        // 투사체 이동
        if (useHoming && _target != null && _target.gameObject.activeSelf)
        {
            // 유도 이동
            Vector3 direction = (_target.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, homingStrength * Time.deltaTime);
        }

        // 전방 이동
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!_isActive) return;

        // Check if hit an enemy
        MonsterBase monster = other.GetComponent<MonsterBase>();
        if (monster != null && monster.gameObject.activeSelf)
        {
            OnHitEnemy(monster);
        }
    }

    // 투사체가 적에게 맞았을 때 호출됨
    protected virtual void OnHitEnemy(MonsterBase enemy)
    {
        // 데미지 적용
        enemy.TakeDamage(_damage);

        string critText = _isCritical ? " [치명타]" : "";
        Debug.Log($"[Projectile] {enemy.gameObject.name} 타격: {_damage:F1} 데미지{critText}");

        // 타격 이펙트 생성
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // 풀로 반환
        ReturnToPool();
    }

    // 투사체를 풀로 반환
    protected virtual void ReturnToPool()
    {
        _isActive = false;
        _target = null;
        PoolManager.Instance.ReturnPool(this);
    }

    // 풀에서 투사체를 생성하는 정적 헬퍼
    public static T Spawn<T>(
        T prefab,
        Vector3 position,
        MonsterBase target,
        float damage,
        bool isCritical) where T : ProjectileBase
    {
        // 풀에서 가져오기 (없으면 생성)
        T projectile = PoolManager.Instance.GetFromPool(prefab);

        if (projectile == null)
        {
            // 풀이 비었으므로 먼저 생성
            PoolManager.Instance.CreatePool(prefab, 10);
            projectile = PoolManager.Instance.GetFromPool(prefab);
        }

        if (projectile != null)
        {
            projectile.transform.position = position;
            projectile.Initialize(target, damage, isCritical);
        }

        return projectile;
    }
}

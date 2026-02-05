using UnityEngine;

// 플레이어의 적 탐지 및 타겟팅을 처리함
public class PlayerTargeting : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float detectionRadius = 15f;

    private PlayerCombatStats _playerStats;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerCombatStats>();
        if (_playerStats != null)
        {
            detectionRadius = _playerStats.DetectionRange;
        }
    }

    // 감지 범위 내에서 살아있는 가장 가까운 적을 찾음
    // 반환값: 가장 가까운 Enemy MonsterBase, 없으면 null
    public MonsterBase FindClosestEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);

        MonsterBase closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (var collider in colliders)
        {
            MonsterBase monster = collider.GetComponent<MonsterBase>();

            // 몬스터가 아니거나 이미 죽은 경우 건너뜀
            if (monster == null || monster.gameObject == null || !monster.gameObject.activeSelf)
                continue;

            float distance = Vector3.Distance(transform.position, collider.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = monster;
            }
        }

        return closestEnemy;
    }

    // 타겟이 여전히 유효한지 확인 (살아있고 범위 내에 있음)
    public bool IsTargetValid(MonsterBase target, float maxRange)
    {
        if (target == null || target.gameObject == null || !target.gameObject.activeSelf)
            return false;

        float distance = Vector3.Distance(transform.position, target.transform.position);
        return distance <= maxRange;
    }

    // 에디터에서 감지 범위 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

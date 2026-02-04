using UnityEngine;

namespace Combat.Drop
{
    /// <summary>
    /// 드롭 아이템 스폰 및 관리
    /// CombatManager에서 초기화되어 사용
    /// </summary>
    public class DropManager : MonoBehaviour
    {
        public static DropManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private DropItem goldDropPrefab;
        [SerializeField] private DropItem expDropPrefab;

        [Header("Pool Settings")]
        [SerializeField] private int poolInitCount = 20;

        [Header("Drop Settings")]
        [SerializeField] private float dropSpreadRadius = 0.5f;
        
        private int _defaultGoldDropCount = 1;
        private int _defaultExpDropCount = 1;

        private Transform _playerTransform;
        private bool _isInitialized;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// DropManager 초기화 - CombatManager에서 호출
        /// </summary>
        public void Initialize(Transform player)
        {
            Debug.Log("[DropManager] Initializing...");
            _playerTransform = player;

            // 골드 드롭 풀 생성
            if (goldDropPrefab != null)
            {
                PoolManager.Instance.CreatePool(goldDropPrefab, poolInitCount, transform);
            }
            else
            {
                Debug.LogWarning("[DropManager] Gold drop prefab is not assigned.");
            }

            // 경험치 드롭 풀 생성
            if (expDropPrefab != null)
            {
                PoolManager.Instance.CreatePool(expDropPrefab, poolInitCount, transform);
            }
            else
            {
                Debug.LogWarning("[DropManager] Exp drop prefab is not assigned.");
            }

            _isInitialized = true;
            Debug.Log("[DropManager] Initialized.");
        }

        /// <summary>
        /// 골드 드롭 아이템 1개 생성
        /// </summary>
        public void SpawnGoldDrop(Vector3 position, long amount)
        {
            if (!_isInitialized || goldDropPrefab == null) return;

            DropItem drop = PoolManager.Instance.GetFromPool(goldDropPrefab);
            if (drop != null)
            {
                drop.transform.position = position;
                drop.Initialize(_playerTransform, amount);
            }
        }

        /// <summary>
        /// 경험치 드롭 아이템 1개 생성
        /// </summary>
        public void SpawnExpDrop(Vector3 position, long amount)
        {
            if (!_isInitialized || expDropPrefab == null) return;

            DropItem drop = PoolManager.Instance.GetFromPool(expDropPrefab);
            if (drop != null)
            {
                drop.transform.position = position;
                drop.Initialize(_playerTransform, amount);
            }
        }

        /// <summary>
        /// 골드와 경험치 드롭 아이템을 여러 개로 분산하여 생성
        /// </summary>
        /// <param name="position">드롭 위치 (몬스터 사망 위치)</param>
        /// <param name="goldAmount">총 골드량</param>
        /// <param name="expAmount">총 경험치량</param>
        /// <param name="goldCount">골드 드롭 개수 (기본값: 1)</param>
        /// <param name="expCount">경험치 드롭 개수 (기본값: 1)</param>
        public void SpawnDrops(Vector3 position, long goldAmount, int expAmount, int goldCount = -1, int expCount = -1)
        {
            if (!_isInitialized) return;

            // 기본값 적용
            if (goldCount < 0) goldCount = _defaultGoldDropCount;
            if (expCount < 0) expCount = _defaultExpDropCount;

            // 골드 드롭 생성
            if (goldAmount > 0 && goldCount > 0)
            {

                Vector3 spawnPos = GetRandomSpreadPosition(position);
                SpawnGoldDrop(spawnPos, 1);
            }

            // 경험치 드롭 생성
            if (expAmount > 0 && expCount > 0)
            {
                Vector3 spawnPos = GetRandomSpreadPosition(position);
                SpawnExpDrop(spawnPos, 1);
            }
        }

        /// <summary>
        /// 기준 위치에서 랜덤하게 분산된 위치 반환
        /// </summary>
        private Vector3 GetRandomSpreadPosition(Vector3 basePosition)
        {
            Vector2 randomOffset = Random.insideUnitCircle * dropSpreadRadius;
            return basePosition + new Vector3(randomOffset.x, randomOffset.y, 0);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}

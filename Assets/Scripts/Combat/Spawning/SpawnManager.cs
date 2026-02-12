using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// 몬스터 및 아이템 스폰 관리
// Addressables를 사용하여 풀링된 오브젝트를 로드하고 생성
public class SpawnManager : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int maxQueueSize = 20;
    [SerializeField] private float queueSpacing = 2f;
    [SerializeField] private float queueBaseOffset = 1.5f;
    [SerializeField] private string commonMonsterAddress = "CommonMonster"; // 공통 몬스터 Addressable 키
    [SerializeField] private string bossMonsterAddress = "BossMonster"; // 보스 몬스터 Addressable 키
    [SerializeField] private string rewardBoxId = "REWARD_BOX"; // 보상 상자 몬스터 ID

    private Coroutine _spawnCoroutine;
    private StageData _currentStageData;
    private Transform _playerTransform;

    // 활성화된 몬스터 추적 (Queue)
    // 인덱스 0이 가장 앞에 있는 몬스터 (플레이어와 가장 가까움)
    private List<MonsterBase> _enemyQueue = new List<MonsterBase>();
    private bool _poolsInitialized = false;
    private int _totalSpawnedCount = 0;

    // Addressable 로드된 프리팹 캐시
    private Dictionary<string, GameObject> _loadedPrefabs = new Dictionary<string, GameObject>();

    public void Initialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    public void InitializePools(StageData stageData)
    {
        if (stageData == null) return;

        StartCoroutine(InitializePoolsAsync(stageData));
    }

    private IEnumerator InitializePoolsAsync(StageData stageData)
    {
        // 공통 몬스터 프리팹 로드
        if (!string.IsNullOrEmpty(commonMonsterAddress))
        {
            yield return LoadPrefabAsync(commonMonsterAddress, 10);
        }

        // 보스 몬스터 프리팹 로드
        if (!string.IsNullOrEmpty(bossMonsterAddress))
        {
            yield return LoadPrefabAsync(bossMonsterAddress, 2);
        }

        _poolsInitialized = true;
        Debug.Log("[SpawnManager] 풀 초기화됨 (공통 몬스터 + 보스 몬스터)");
    }

    private IEnumerator LoadPrefabAsync(string address, int poolSize)
    {
        if (_loadedPrefabs.ContainsKey(address)) yield break;

        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(address);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject prefab = handle.Result;
            _loadedPrefabs[address] = prefab;

            MonsterBase monsterBase = prefab.GetComponent<MonsterBase>();
            if (monsterBase != null)
            {
                PoolManager.Instance.CreatePool(monsterBase, poolSize, transform);
            }
        }
        else
        {
            Debug.LogError($"[SpawnManager] 프리팹 로드 실패: {address}");
        }
    }

    public void StartFarmingSpawn(StageData stageData)
    {
        _currentStageData = stageData;

        if (!_poolsInitialized)
        {
            InitializePools(stageData);
        }

        _totalSpawnedCount = 0;

        StopSpawning();
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }

    public void SpawnRewardBox()
    {
        StopSpawning();

        if (string.IsNullOrEmpty(rewardBoxId))
        {
            Debug.LogWarning("[SpawnManager] 보상 상자 ID가 설정되지 않음.");
            return;
        }

        MonsterData boxData = DataManager.Instance.monsters.Get(rewardBoxId);
        if (boxData == null)
        {
            Debug.LogError($"[SpawnManager] 보상 상자 데이터 없음 ID: {rewardBoxId}");
            return;
        }

        MonsterBase rewardBox = SpawnMonster(boxData, commonMonsterAddress, GetSpawnPosition() + Vector3.right * 5);
        if (rewardBox != null)
        {
            Debug.Log($"[SpawnManager] 보상 상자 소환: {boxData.name} ({boxData.id})");
        }
    }

    public void SpawnBoss()
    {
        StopSpawning();
        CleanUpEnemies();

        MonsterData bossData = DataManager.Instance.monsters.Get(_currentStageData.bossId);
        if (bossData == null)
        {
            Debug.LogError($"[SpawnManager] 보스 데이터 없음: {_currentStageData.bossId}");
            return;
        }

        MonsterBase boss = SpawnMonster(bossData, bossMonsterAddress, GetSpawnPosition() + Vector3.right * 3);
        if (boss != null)
        {
            Debug.Log($"[SpawnManager] 보스 소환: {bossData.name} ({bossData.id})");
        }
    }

    public void CleanUpEnemies()
    {
        for (int index = _enemyQueue.Count - 1; index >= 0; index--)
        {
            if (_enemyQueue[index] != null)
            {
                PoolManager.Instance.ReturnPool(_enemyQueue[index]);
            }
        }
        _enemyQueue.Clear();
    }

    private void Update()
    {
        UpdateQueuePositions();
    }

    private void UpdateQueuePositions()
    {
        if (_playerTransform == null) return;

        for (int i = 0; i < _enemyQueue.Count; i++)
        {
            MonsterBase enemy = _enemyQueue[i];
            if (enemy == null) continue;

            // 타겟 위치: 플레이어 + 오른쪽 * (기본 + 인덱스 * 간격)
            // XZ 평면에서의 2D 로직 가정 (X가 수평)
            Vector3 targetPos = _playerTransform.position + Vector3.right * (queueBaseOffset + i * queueSpacing);

            // Y 위치는 동일하게 유지하거나 지면 높이로 설정
            targetPos.y = enemy.transform.position.y;
            targetPos.z = _playerTransform.position.z;

            enemy.SetTargetPosition(targetPos);
        }
    }

    private IEnumerator SpawnRoutine()
    {
        // 풀 초기화 대기
        yield return new WaitUntil(() => _poolsInitialized);

        // 초기 스폰 배치: 큐를 즉시 채움
        int initialNeeded = maxQueueSize - _enemyQueue.Count;

        // 전체 제한에 따라 초기 필요량 스폰 가능 여부 확인
        if (_currentStageData != null)
        {
            int remainingToSpawn = _currentStageData.monsterCount - _totalSpawnedCount;
            if (initialNeeded > remainingToSpawn)
            {
                initialNeeded = remainingToSpawn;
            }
        }

        for (int i = 0; i < initialNeeded; i++)
        {
            if (_currentStageData != null && !string.IsNullOrEmpty(_currentStageData.monsterId))
            {
                MonsterData monsterData = DataManager.Instance.monsters.Get(_currentStageData.monsterId);
                if (monsterData != null)
                {
                    SpawnEnemy(monsterData);
                }
            }
        }

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // 전체 제한 확인
            if (_currentStageData != null && _totalSpawnedCount >= _currentStageData.monsterCount)
            {
                yield break; // 제한 도달 시 스폰 중지
            }

            if (_enemyQueue.Count < maxQueueSize)
            {
                if (_currentStageData != null && !string.IsNullOrEmpty(_currentStageData.monsterId))
                {
                    MonsterData monsterData = DataManager.Instance.monsters.Get(_currentStageData.monsterId);
                    if (monsterData != null)
                    {
                        SpawnEnemy(monsterData);
                    }
                }
            }
        }
    }

    private void SpawnEnemy(MonsterData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[SpawnManager] 스폰 불가: 데이터 누락");
            return;
        }

        MonsterBase enemy = SpawnMonster(data, commonMonsterAddress, GetSpawnPosition());
        if (enemy != null)
        {
            _totalSpawnedCount++;
            Debug.Log($"[SpawnManager] {data.name} 소환됨. 총 소환: {_totalSpawnedCount}/{_currentStageData.monsterCount}");
        }
    }

    // 공통 몬스터 스폰 로직
    // 프리팹 조회 → 풀 가져오기 → 위치 설정 → 초기화 → flipX → 레이어 설정 → 큐 추가
    private MonsterBase SpawnMonster(MonsterData data, string prefabAddress, Vector3 position)
    {
        if (!_loadedPrefabs.TryGetValue(prefabAddress, out GameObject prefab))
        {
            Debug.LogWarning($"[SpawnManager] 프리팹 로드 안됨: {prefabAddress}");
            return null;
        }

        MonsterBase prefabMonster = prefab.GetComponent<MonsterBase>();
        if (prefabMonster == null)
        {
            Debug.LogError($"[SpawnManager] 프리팹 {prefab.name}에 MonsterBase 컴포넌트 없음");
            return null;
        }

        MonsterBase monster = PoolManager.Instance.GetFromPool(prefabMonster);
        if (monster == null) return null;

        monster.transform.position = position;
        monster.Initialize(data, _playerTransform);

        // 몬스터 스프라이트를 왼쪽(플레이어 방향)으로 전환
        SpriteRenderer spriteRenderer = monster.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.flipX = true;

        _enemyQueue.Add(monster);
        return monster;
    }

    private Vector3 GetSpawnPosition()
    {
        if (_playerTransform == null) return Vector3.zero;

        float spawnXOffset = queueBaseOffset + (_enemyQueue.Count * queueSpacing) + queueSpacing;

        Vector3 spawnPos = _playerTransform.position + Vector3.right * spawnXOffset;
        spawnPos.z = _playerTransform.position.z;
        return spawnPos;
    }

    public void UnregisterEnemy(MonsterBase enemy)
    {
        if (_enemyQueue.Contains(enemy))
        {
            _enemyQueue.Remove(enemy);

            // 죽은 적이 보상 상자라면, 다른 것을 소환하지 않음.
            if (enemy.IsRewardBox)
            {
                Debug.Log("[SpawnManager] 보상 상자 처치됨. 스테이지 재시작.");
                // 파밍 스폰을 재시작하면 카운트와 루프가 초기화됨
                StartFarmingSpawn(_currentStageData);
                return;
            }

            // 스테이지의 모든 몬스터가 클리어되었는지 확인
            if (_currentStageData != null && _totalSpawnedCount >= _currentStageData.monsterCount && _enemyQueue.Count == 0)
            {
                SpawnRewardBox();
            }
        }
    }

    public MonsterBase GetFirstEnemy()
    {
        if (_enemyQueue.Count > 0)
        {
            return _enemyQueue[0];
        }
        return null;
    }

    private void OnDestroy()
    {
        // Addressable 리소스 해제
        foreach (var kvp in _loadedPrefabs)
        {
            Addressables.Release(kvp.Value);
        }
        _loadedPrefabs.Clear();
    }
}
